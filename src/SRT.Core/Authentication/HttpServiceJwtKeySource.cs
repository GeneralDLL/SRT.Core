using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SRT.Core.Domain;
using SRT.Core.Domain.RequestsDomqins;
using SRT.Core.Interfaces;

namespace SRT.Core.Authentication;

public sealed class HttpServiceJwtKeySource : IServiceJwtKeySource
{
    private readonly URLData _urls;
    private readonly AppSetting _app;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HttpServiceJwtKeySource> _logger;

    public HttpServiceJwtKeySource(URLData urls, AppSetting app, IConfiguration configuration, ILogger<HttpServiceJwtKeySource> logger)
    {
        _urls = urls;
        _app = app;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ServiceJwtKeySet> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(_urls.Authentication, UriKind.Absolute, out var uri))
            return ServiceJwtKeySet.Failed();

        if (!uri.AbsoluteUri.EndsWith('/'))
            uri = new Uri(uri.AbsoluteUri + "/");

        using var client = new HttpClient { BaseAddress = uri };
        using var request = new HttpRequestMessage(HttpMethod.Get, "internal/service-auth/Keys");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ISecuritySystem.ResolveInternalToken(null));
        request.Headers.TryAddWithoutValidation("X-Service-Id", _app.ServiceId.ToString(CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation("X-Service-Secret", ISecuritySystem.ResolveServiceSecret(_configuration, _app.ServiceId));

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ServiceJwtKeySet.Failed();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var envelope = JsonConvert.DeserializeObject<ResponseData>(body);
            var keys = envelope?.GetData<List<ServiceJwtKeyWire>>();
            if (keys is null)
                return ServiceJwtKeySet.Failed();

            return ServiceJwtKeySet.Success(keys.Select(key => new ServiceJwtPublicKey
            {
                Kid = key.kid,
                Alg = string.IsNullOrWhiteSpace(key.alg) ? ServiceJwtPublicKey.Es256 : key.alg,
                PublicKeySpki = key.publicKey,
                Active = key.active
            }).ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetServiceKeys failed.");
            return ServiceJwtKeySet.Failed();
        }
    }

    private sealed class ServiceJwtKeyWire
    {
        public string kid { get; set; } = string.Empty;
        public string alg { get; set; } = string.Empty;
        public string publicKey { get; set; } = string.Empty;
        public bool active { get; set; }
    }
}
