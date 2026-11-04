using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.Authentication;
using SRT.Core.Domain.DomainInterfaces;
using SRT.Core.ENV;

namespace SRT.Core.Domain.Diagnostics
{
    /// <summary>Builds the diagnostic envelope of a failed HTTP request. The request body is never stored.</summary>
    public static class HttpDiagnosticEnvelope
    {
        private const string FlowCookiePrefix = "p2e_flow_";

        private static readonly HashSet<string> SecretHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Proxy-Authorization", "X-Api-Key"
        };

        private static readonly string? AppVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

        public static DiagnosticEnvelope Create(HttpContext context, int statusCode)
        {
            var request = context.Request;
            var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var user = context.Items["UserData"] as IUserInfo;
            var services = context.RequestServices;
            var setting = services.GetService<AppSetting>();

            var envelope = new DiagnosticEnvelope
            {
                CorrelationId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
                Environment = services.GetService<ISystemENV>()?.environment.ToString(),
                Version = AppVersion,
                Category = DiagnosticCategories.Http,
                Operation = $"{request.Method} {route ?? request.Path.Value}",
                Severity = "Error",
                Outcome = DiagnosticOutcomes.Failed,
                Target = request.Path.Value,
                Detail = new DiagnosticDetail
                {
                    HttpMethod = request.Method,
                    Path = request.Path.Value,
                    QueryString = request.QueryString.HasValue ? request.QueryString.Value : null,
                    RouteTemplate = route,
                    StatusCode = statusCode,
                    UserId = user?.id,
                    UserType = user?.userType,
                    ActiveRole = context.Items[DashboardAccess.ActiveRoleItemKey] as string,
                    ClientIp = ClientIp(context),
                    UserAgent = NullIfEmpty(request.Headers.UserAgent.ToString()),
                    Referer = NullIfEmpty(request.Headers.Referer.ToString()),
                    FlowTraceId = FlowTraceId(request),
                    Headers = request.Headers
                        .Where(header => !SecretHeaders.Contains(header.Key))
                        .ToDictionary(header => header.Key, header => header.Value.ToString()),
                },
            };

            if (!string.IsNullOrWhiteSpace(setting?.ServiceName))
                envelope.Service = setting.ServiceName;

            return envelope;
        }

        private static string? ClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            return forwarded.Length > 0 ? forwarded : context.Connection.RemoteIpAddress?.ToString();
        }

        // The gateway keeps a W3C traceparent in the flow cookie: 00-<trace id>-<span id>-<flags>.
        private static string? FlowTraceId(HttpRequest request)
        {
            var cookie = request.Cookies.FirstOrDefault(item => item.Key.StartsWith(FlowCookiePrefix, StringComparison.Ordinal));
            return cookie.Value?.Split('-') is [_, var traceId, ..] ? traceId : null;
        }

        private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
    }
}
