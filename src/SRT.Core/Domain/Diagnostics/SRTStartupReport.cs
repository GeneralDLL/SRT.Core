// Ignore Spelling: SRT

using System.Collections;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SRT.Core.Domain.Diagnostics
{
    /// <summary>
    /// Collects what each startup step did and writes it as ONE log record (category <see cref="LoggerCategory"/>).
    /// Success is written at Warning so it survives a Production "Default: Warning" filter; any FAILED entry
    /// or a startup exception writes it at Error. Never put a password or a full connection string in an entry.
    /// </summary>
    public sealed class SRTStartupReport
    {
        public const string LoggerCategory = "SRT.Startup";
        public static readonly EventId StartupEventId = new(1000, "SRTStartup");

        public static class Status
        {
            public const string Ok = "OK";
            public const string Off = "OFF";
            public const string Failed = "FAILED";
            public const string Skipped = "SKIPPED";
            public const string Info = "INFO";
        }

        public sealed record Entry(string Section, string Status, string Detail);

        private readonly object _gate = new();
        private readonly List<Entry> _entries = new();
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private int _written;

        public IReadOnlyList<Entry> Entries
        {
            get { lock (_gate) return _entries.ToList(); }
        }

        public bool HasFailure
        {
            get { lock (_gate) return _entries.Any(e => e.Status == Status.Failed); }
        }

        public bool IsWritten => Volatile.Read(ref _written) == 1;

        /// <summary>Adds or replaces the entry for <paramref name="section"/>. Section is a short key such as "sql.UsersDbContext".</summary>
        public void Set(string section, string status, string detail)
        {
            if (string.IsNullOrWhiteSpace(section))
                return;

            var entry = new Entry(section.Trim(), status, detail ?? "");
            lock (_gate)
            {
                var index = _entries.FindIndex(e => e.Section == entry.Section);
                if (index >= 0)
                    _entries[index] = entry;
                else
                    _entries.Add(entry);
            }
        }

        public void Ok(string section, string detail) => Set(section, Status.Ok, detail);
        public void Off(string section, string detail = "") => Set(section, Status.Off, detail);
        public void Failed(string section, string detail) => Set(section, Status.Failed, detail);
        public void Info(string section, string detail) => Set(section, Status.Info, detail);

        /// <summary>Appends text to an existing entry's detail (or creates an INFO entry).</summary>
        public void Append(string section, string detail)
        {
            lock (_gate)
            {
                var index = _entries.FindIndex(e => e.Section == section);
                if (index < 0)
                {
                    _entries.Add(new Entry(section, Status.Info, detail));
                    return;
                }

                var current = _entries[index];
                var joined = string.IsNullOrEmpty(current.Detail) ? detail : current.Detail + "; " + detail;
                _entries[index] = current with { Detail = joined };
            }
        }

        /// <summary>
        /// Writes the summary once. Later calls are ignored, so the failure path and ApplicationStarted cannot both write.
        /// </summary>
        public bool Write(ILogger logger, string serviceName, string environment, string? urls = null, Exception? failure = null)
        {
            if (Interlocked.Exchange(ref _written, 1) == 1)
                return false;

            var entries = Entries;
            var failed = failure is not null || entries.Any(e => e.Status == Status.Failed);
            var outcome = failure is not null ? "FAILED" : failed ? "DEGRADED" : "OK";
            var level = failed ? LogLevel.Error : LogLevel.Warning;
            var elapsedMs = (long)_watch.Elapsed.TotalMilliseconds;
            var version = typeof(SRTStartupReport).Assembly.GetName().Version?.ToString() ?? "";
            var appVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "";

            var text = new StringBuilder();
            text.Append("SRT startup ").Append(outcome).Append(": ").Append(serviceName)
                .Append(" env=").Append(environment)
                .Append(" version=").Append(appVersion)
                .Append(" took=").Append(elapsedMs).Append("ms");
            if (!string.IsNullOrWhiteSpace(urls))
                text.AppendLine().Append("  urls: ").Append(urls);

            var width = entries.Count == 0 ? 0 : entries.Max(e => e.Section.Length);
            foreach (var entry in entries)
            {
                text.AppendLine()
                    .Append("  ").Append(entry.Section.PadRight(width))
                    .Append(" : ").Append(entry.Status.PadRight(7))
                    .Append(' ').Append(entry.Detail);
            }

            if (failure is not null)
                text.AppendLine().Append("  error: ").Append(failure.GetType().Name).Append(": ").Append(failure.Message);

            var attributes = new List<KeyValuePair<string, object?>>
            {
                new("srt.startup.outcome", outcome),
                new("srt.startup.service", serviceName),
                new("srt.startup.environment", environment),
                new("srt.startup.elapsed_ms", elapsedMs),
                new("srt.startup.app_version", appVersion),
                new("srt.startup.core_version", version),
            };
            if (!string.IsNullOrWhiteSpace(urls))
                attributes.Add(new("srt.startup.urls", urls));
            foreach (var entry in entries)
            {
                var key = "srt.startup." + NormalizeKey(entry.Section);
                attributes.Add(new(key + ".status", entry.Status));
                if (!string.IsNullOrEmpty(entry.Detail))
                    attributes.Add(new(key + ".detail", entry.Detail));
            }

            var state = new StartupLogState(attributes, text.ToString());
            logger.Log(level, StartupEventId, state, failure, static (s, _) => s.Message);
            return true;
        }

        private static string NormalizeKey(string section)
        {
            var builder = new StringBuilder(section.Length);
            foreach (var ch in section)
                builder.Append(char.IsLetterOrDigit(ch) || ch == '.' ? char.ToLowerInvariant(ch) : '_');
            return builder.ToString();
        }

        /// <summary>
        /// Structured state: OpenTelemetry (ParseStateValues=true) turns every pair into a log attribute,
        /// and the formatter returns the multi-line text for the console and the Aspire message column.
        /// </summary>
        private sealed class StartupLogState : IReadOnlyList<KeyValuePair<string, object?>>
        {
            private readonly List<KeyValuePair<string, object?>> _items;

            public StartupLogState(List<KeyValuePair<string, object?>> items, string message)
            {
                _items = items;
                Message = message;
            }

            public string Message { get; }
            public int Count => _items.Count;
            public KeyValuePair<string, object?> this[int index] => _items[index];
            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _items.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public override string ToString() => Message;
        }
    }
}
