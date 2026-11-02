using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SRT.Core.Interfaces
{
    public interface ISecuritySystem
    {
        protected string EnvOrDefault(string envName, string fallback)
        {
            var v = Environment.GetEnvironmentVariable(envName);
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }

        /// <summary>
        /// Maps a compile-time attribute baseToken to the runtime (env-overridable) secret.
        /// </summary>
        static string ResolveInternalToken(string? attributeBaseToken)
        {
            // default: env S2S_TOKEN or the compile-time value
            var env = Environment.GetEnvironmentVariable("S2S_TOKEN");
            return string.IsNullOrWhiteSpace(env) ? (attributeBaseToken ?? "") : env;
        }

        /// <summary>
        /// This service's secret for Authentication: env AUTH_SERVICE_SECRET_{serviceId}, else AUTH_SERVICE_SECRET.
        /// </summary>
        static string ResolveServiceSecret(int serviceId)
        {
            var own = Environment.GetEnvironmentVariable($"AUTH_SERVICE_SECRET_{serviceId}");
            return string.IsNullOrWhiteSpace(own)
                ? Environment.GetEnvironmentVariable("AUTH_SERVICE_SECRET") ?? string.Empty
                : own;
        }

        /// <summary>
        /// This service's secret for Authentication from configuration: AUTH_SERVICE_SECRET_{serviceId}, else AUTH_SERVICE_SECRET.
        /// The host adds environment variables after appsettings, so they win. A blank value counts as missing.
        /// </summary>
        static string ResolveServiceSecret(IConfiguration configuration, int serviceId)
        {
            var own = configuration[$"AUTH_SERVICE_SECRET_{serviceId}"];
            return string.IsNullOrWhiteSpace(own)
                ? configuration["AUTH_SERVICE_SECRET"] ?? string.Empty
                : own;
        }
    }
}
