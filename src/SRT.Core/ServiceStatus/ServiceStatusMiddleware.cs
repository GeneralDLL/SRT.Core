// Ignore Spelling: SRT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.ENV;

namespace SRT.Core.ServiceStatus
{
    /// <summary>
    /// Serves GET "/" with environment, start time, uptime, and health as JSON.
    /// Placed before Swagger UI so Production (Swagger RoutePrefix = "") does not swallow "/".
    /// </summary>
    public sealed class ServiceStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceLifecycle _lifecycle;
        private readonly ISystemENV _systemEnv;

        public ServiceStatusMiddleware(
            RequestDelegate next,
            ServiceLifecycle lifecycle,
            ISystemENV systemEnv)
        {
            _next = next;
            _lifecycle = lifecycle;
            _systemEnv = systemEnv;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsRootGet(context.Request))
            {
                var uptime = _lifecycle.GetUptime();
                var payload = new ServiceStatusResponse
                {
                    Environment = MapCanonicalEnvironment(_systemEnv.environment),
                    EnvironmentName = _systemEnv.environment.ToString(),
                    StartedAt = _lifecycle.StartedAtUtc,
                    Uptime = uptime.ToString(@"d\.hh\:mm\:ss"),
                    UptimeSeconds = (long)uptime.TotalSeconds,
                    Status = "Healthy"
                };

                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(payload);
                return;
            }

            await _next(context);
        }

        private static bool IsRootGet(HttpRequest request)
        {
            return HttpMethods.IsGet(request.Method)
                && (request.Path == PathString.Empty || request.Path == "/");
        }

        /// <summary>
        /// Maps project-specific environments to development / staging / production.
        /// </summary>
        internal static string MapCanonicalEnvironment(SystemENV.EnvironmentType environment)
        {
            return environment switch
            {
                SystemENV.EnvironmentType.Production or SystemENV.EnvironmentType.ProductionIIS
                    => "production",
                SystemENV.EnvironmentType.Preview
                    => "staging",
                _ => "development"
            };
        }
    }

    public static class ServiceStatusExtensions
    {
        /// <summary>
        /// Registers <see cref="ServiceLifecycle"/> and the root status middleware.
        /// Call once during host configuration (before Swagger UI).
        /// </summary>
        public static IServiceCollection AddServiceStatus(this IServiceCollection services)
        {
            services.AddSingleton(new ServiceLifecycle());
            return services;
        }

        /// <summary>
        /// Maps GET "/" to the JSON service status response.
        /// </summary>
        public static IApplicationBuilder UseServiceStatus(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ServiceStatusMiddleware>();
        }
    }
}