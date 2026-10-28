using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRT.Core.Authentication;

public sealed class ServiceJwtKeyCache
{
    private readonly object _gate = new();
    private readonly IServiceJwtKeySource _source;
    private readonly IOptions<ServiceAuthOptions> _options;
    private readonly IServiceTokenClock _clock;
    private readonly ILogger<ServiceJwtKeyCache> _logger;
    private Dictionary<string, ServiceJwtPublicKey> _keys = new(StringComparer.Ordinal);
    private DateTime _fetchedAtUtc;
    private DateTime _attemptAtUtc;
    private Task<bool>? _flight;

    public ServiceJwtKeyCache(
        IServiceJwtKeySource source,
        IOptions<ServiceAuthOptions> options,
        IServiceTokenClock clock,
        ILogger<ServiceJwtKeyCache> logger)
    {
        _source = source;
        _options = options;
        _clock = clock;
        _logger = logger;
    }

    public async Task WarmAsync()
    {
        try
        {
            await RefreshAsync(ignoreMinInterval: true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service JWT key warmup failed.");
        }
    }

    public async Task<ServiceJwtKeyLookup> ResolveAsync(string kid, CancellationToken cancellationToken)
    {
        #region #S2S-DEST-06 Kid in cache
        if (TryGet(kid, out var cached) && IsFresh())
            return ServiceJwtKeyLookup.Found(cached);
        #endregion

        #region #S2S-DEST-07 Refresh
        var periodic = false;
        if (!IsFresh())
            periodic = await RefreshAsync(ignoreMinInterval: true, cancellationToken);
        #endregion

        if (TryGet(kid, out cached))
            return ServiceJwtKeyLookup.Found(cached);

        if (periodic)
        {
            #region #S2S-DEST-09 Kid after refresh
            return ServiceJwtKeyLookup.Unknown();
            #endregion
        }

        var forced = await RefreshAsync(ignoreMinInterval: false, cancellationToken);
        if (TryGet(kid, out cached))
            return ServiceJwtKeyLookup.Found(cached);

        if (forced)
            return ServiceJwtKeyLookup.Unknown();

        #region #S2S-DEST-08 Keys unavailable
        return ServiceJwtKeyLookup.Unavailable();
        #endregion
    }

    private bool TryGet(string kid, out ServiceJwtPublicKey key)
    {
        lock (_gate)
            return _keys.TryGetValue(kid, out key!);
    }

    private bool IsFresh()
    {
        lock (_gate)
        {
            if (_fetchedAtUtc == default)
                return false;

            var interval = TimeSpan.FromMinutes(_options.Value.KeysRefreshIntervalMinutes);
            return _clock.UtcNow < _fetchedAtUtc.Add(interval);
        }
    }

    private async Task<bool> RefreshAsync(bool ignoreMinInterval, CancellationToken cancellationToken)
    {
        Task<bool> flight;
        lock (_gate)
        {
            if (_flight is not null)
            {
                flight = _flight;
            }
            else if (!ignoreMinInterval && _attemptAtUtc != default
                && _clock.UtcNow < _attemptAtUtc.Add(TimeSpan.FromSeconds(_options.Value.KeysRefreshMinIntervalSeconds)))
            {
                return false;
            }
            else
            {
                _attemptAtUtc = _clock.UtcNow;
                _flight = RefreshCoreAsync();
                flight = _flight;
            }
        }

        return await flight.WaitAsync(cancellationToken);
    }

    private async Task<bool> RefreshCoreAsync()
    {
        try
        {
            var set = await _source.GetAsync(CancellationToken.None);
            if (!set.Succeeded)
                return false;

            var map = new Dictionary<string, ServiceJwtPublicKey>(StringComparer.Ordinal);
            foreach (var key in set.Keys)
            {
                if (!string.IsNullOrWhiteSpace(key.Kid))
                    map[key.Kid] = key;
            }

            lock (_gate)
            {
                _keys = map;
                _fetchedAtUtc = _clock.UtcNow;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Service JWT key refresh failed.");
            return false;
        }
        finally
        {
            lock (_gate)
                _flight = null;
        }
    }
}

public sealed class ServiceJwtKeyWarmup : IHostedService
{
    private readonly ServiceJwtKeyCache _cache;

    public ServiceJwtKeyWarmup(ServiceJwtKeyCache cache)
    {
        _cache = cache;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = _cache.WarmAsync();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
