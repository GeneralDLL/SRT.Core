// Ignore Spelling: SRT Uptime

namespace SRT.Core.ServiceStatus
{
    /// <summary>
    /// Captures the service boot timestamp so root "/" can report start time and uptime.
    /// Registered as a singleton at application startup.
    /// </summary>
    public sealed class ServiceLifecycle
    {
        /// <summary>
        /// UTC instant when this singleton was created (application boot).
        /// </summary>
        public DateTimeOffset StartedAtUtc { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Elapsed time since <see cref="StartedAtUtc"/>.
        /// </summary>
        public TimeSpan GetUptime() => DateTimeOffset.UtcNow - StartedAtUtc;
    }
}