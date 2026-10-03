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
    }
}
