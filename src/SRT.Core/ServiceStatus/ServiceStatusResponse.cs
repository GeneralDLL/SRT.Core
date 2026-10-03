// Ignore Spelling: SRT

namespace SRT.Core.ServiceStatus
{
    /// <summary>
    /// JSON payload returned by GET "/" — environment, uptime, and health.
    /// </summary>
    public sealed class ServiceStatusResponse
    {
        /// <summary>
        /// Canonical environment: development | staging | production.
        /// Derived from ASPNETCORE_ENVIRONMENT via <see cref="ENV.SystemENV"/>.
        /// </summary>
        public string Environment { get; init; } = "development";

        /// <summary>
        /// Raw project environment name (e.g. LocalSRT, Preview, ProductionIIS).
        /// </summary>
        public string EnvironmentName { get; init; } = string.Empty;

        /// <summary>
        /// UTC time when the process started serving requests.
        /// </summary>
        public DateTimeOffset StartedAt { get; init; }

        /// <summary>
        /// Human-readable uptime (days.hh:mm:ss).
        /// </summary>
        public string Uptime { get; init; } = "00:00:00";

        /// <summary>
        /// Uptime in whole seconds (useful for monitoring scrapers).
        /// </summary>
        public long UptimeSeconds { get; init; }

        /// <summary>
        /// Liveness indicator for this process (Healthy when the host is running).
        /// </summary>
        public string Status { get; init; } = "Healthy";
    }
}