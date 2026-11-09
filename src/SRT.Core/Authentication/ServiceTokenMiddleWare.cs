using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRT.Core.Domain;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.Interfaces;

namespace SRT.Core.Authentication;

public sealed class ServiceTokenMiddleWare
{
    public const string InvalidCode = "SERVICE_TOKEN_INVALID";
    public const string KeysUnavailableCode = "SERVICE_KEYS_UNAVAILABLE";
    public const string CallerItem = "CallerService";

    private readonly ServiceJwtKeyCache _keys;
    private readonly IOptions<ServiceAuthOptions> _options;
    private readonly IServiceTokenClock _clock;
    private readonly AppSetting _app;
    private readonly ILogger<ServiceTokenMiddleWare> _logger;

    public ServiceTokenMiddleWare(
        ServiceJwtKeyCache keys,
        IOptions<ServiceAuthOptions> options,
        IServiceTokenClock clock,
        AppSetting app,
        ILogger<ServiceTokenMiddleWare> logger)
    {
        _keys = keys;
        _options = options;
        _clock = clock;
        _app = app;
        _logger = logger;
    }

    #region #S2S-DEST Invoke
    public async Task InvokeAsync(ActionExecutingContext context, string? token, string? attributeBaseToken)
    {
        #region #S2S-DEST-01 Input
        var options = _options.Value;
        #endregion

        #region #S2S-DEST-02 Bearer
        if (string.IsNullOrWhiteSpace(token))
        {
            // Docker Development has no shared S2S_TOKEN. The action still checks X-Service-Secret.
            if (HasCallerSecret(context))
                return;

            Reject(context);
            return;
        }
        #endregion

        #region #S2S-DEST-03 Legacy token
        var expected = ISecuritySystem.ResolveInternalToken(attributeBaseToken);
        #endregion

        #region #S2S-DEST-04 Legacy accept
        if (!string.IsNullOrEmpty(expected) && string.Equals(token, expected, StringComparison.Ordinal))
        {
            _logger.LogInformation("S2sLegacyTokenAccepted {ServiceName}", _app.ServiceName);
            return;
        }
        #endregion

        if (!ServiceJwtReader.TryRead(token, out var jwt))
        {
            Reject(context);
            return;
        }

        #region #S2S-DEST-05 Algorithm
        if (!string.Equals(jwt.Alg, ServiceJwtPublicKey.Es256, StringComparison.Ordinal)
            || (jwt.HeaderType is not null && !string.Equals(jwt.HeaderType, "JWT", StringComparison.Ordinal)))
        {
            Reject(context);
            return;
        }
        #endregion

        var lookup = await _keys.ResolveAsync(jwt.Kid, context.HttpContext.RequestAborted);
        if (lookup.Kind == ServiceJwtKeyLookupKind.Unavailable)
        {
            #region #S2S-DEST-08 Keys unavailable
            Unavailable(context);
            return;
            #endregion
        }

        if (lookup.Key is null)
        {
            #region #S2S-DEST-09 Kid after refresh
            Reject(context);
            return;
            #endregion
        }

        #region #S2S-DEST-10 Signature
        if (!ServiceJwtReader.Verify(jwt, lookup.Key.PublicKeySpki))
        {
            Reject(context);
            return;
        }
        #endregion

        #region #S2S-DEST-11 Issuer and typ
        if (!string.Equals(jwt.Issuer, options.Issuer, StringComparison.Ordinal)
            || !string.Equals(jwt.PayloadType, "service", StringComparison.Ordinal))
        {
            Reject(context);
            return;
        }
        #endregion

        #region #S2S-DEST-12 Audience
        if (!jwt.Audiences.Contains(_app.ServiceName, StringComparer.Ordinal))
        {
            Reject(context);
            return;
        }
        #endregion

        #region #S2S-DEST-13 Time and subject
        if (!TimingAccepts(jwt, options))
        {
            Reject(context);
            return;
        }
        #endregion

        #region #S2S-DEST-14 CallerService
        context.HttpContext.Items[CallerItem] = jwt.Subject;
        #endregion

        #region #S2S-DEST-15 Continue
        #endregion
    }
    #endregion

    private static bool HasCallerSecret(ActionExecutingContext context)
    {
        var id = context.HttpContext.Request.Headers["X-Service-Id"].ToString();
        var secret = context.HttpContext.Request.Headers["X-Service-Secret"].ToString();
        return int.TryParse(id, out var serviceId) && serviceId > 0 && !string.IsNullOrWhiteSpace(secret);
    }

    private bool TimingAccepts(ServiceJwtDocument jwt, ServiceAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(jwt.Subject))
            return false;

        var now = _clock.UtcNow;
        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(jwt.IssuedAt).UtcDateTime;
        var expires = DateTimeOffset.FromUnixTimeSeconds(jwt.Expires).UtcDateTime;
        var skew = TimeSpan.FromSeconds(options.ClockSkewSeconds);
        var life = TimeSpan.FromSeconds(jwt.Expires - jwt.IssuedAt);
        var maxLife = TimeSpan.FromMinutes(options.MaxTokenLifetimeMinutes);
        if (life <= TimeSpan.Zero || life > maxLife)
            return false;

        if (issuedAt > now.Add(TimeSpan.FromSeconds(options.FutureIssuedAtAllowanceSeconds)))
            return false;

        if (now > expires.Add(skew))
            return false;

        if (jwt.NotBefore is long notBefore && now.Add(skew) < DateTimeOffset.FromUnixTimeSeconds(notBefore).UtcDateTime)
            return false;

        return true;
    }

    #region #S2S-DEST-16 Reject
    private static void Reject(ActionExecutingContext context)
        => SetResult(context, StatusCodes.Status401Unauthorized, InvalidCode);

    private static void Unavailable(ActionExecutingContext context)
        => SetResult(context, StatusCodes.Status503ServiceUnavailable, KeysUnavailableCode);

    private static void SetResult(ActionExecutingContext context, int statusCode, string errorCode)
    {
        context.Result = new ObjectResult(new ResponseData
        {
            status = (HttpStatusCode)statusCode,
            message = errorCode,
            errorCode = errorCode
        })
        {
            StatusCode = statusCode
        };
    }
    #endregion
}
