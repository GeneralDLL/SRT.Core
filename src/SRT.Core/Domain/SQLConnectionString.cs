// Ignore Spelling: DTO SRT SQL Postgre postgress amqp Failover

using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.Domain
{
    public class AppConnectionString
    {
        public GeneralConnectionString DefaultConnection { get; set; } = new();
        public GeneralConnectionString DefaultConnection2 { get; set; } = new();

        public GeneralConnectionString SRTCore_CacheRedis { get; set; } = new();
        public GeneralConnectionString SRTCore_CacheSQL { get; set; } = new();
        public GeneralConnectionString SRTCore_ErrorSQL { get; set; } = new();
        public GeneralConnectionString SRTCore_RabbitMQ { get; set; } = new();

        internal List<GeneralConnectionString> GetAllConnection()
        {
            return this.GetType()
                       .GetProperties()
                       .Where(q => q.PropertyType == typeof(GeneralConnectionString))
                       .Select(q => (GeneralConnectionString)q.GetValue(this)!)
                       .ToList();
        }
    }

    public class GeneralConnectionString
    {
        /// <summary>
        /// Just For help
        /// SQL / Redis / Postgress / RabbitMQ
        /// </summary>
        public string connectionType { get; set; } = "";

        public string host { get; set; } = "";
        public string port { get; set; } = "";

        public string user { get; set; } = "";
        public string pass { get; set; } = "";
        public string dbName { get; set; } = "";

        /// <summary>true / false — TLS for Redis/Rabbit</summary>
        public string sslEnabled { get; set; } = "";

        /// <summary>CA PEM path inside container (Redis/Rabbit TLS)</summary>
        public string caPath { get; set; } = "";

        /// <summary>RabbitMQ virtual host (e.g. app). Empty = /</summary>
        public string virtualHost { get; set; } = "";

        /// <summary>
        /// Per-service name prefix for Redis keys and RabbitMQ queue/exchange names
        /// (for example <c>shop</c> gives <c>shop:...</c>). Empty = names are used as given.
        /// </summary>
        public string pattern { get; set; } = "";

        /// <summary>
        /// <see cref="pattern"/> trimmed; <c>:</c> is added unless it already ends with
        /// <c>:</c>, <c>_</c>, <c>-</c> or <c>.</c>. Empty when no pattern is set.
        /// </summary>
        public string NormalizedPattern => NormalizePattern(pattern);

        /// <summary>Puts <see cref="NormalizedPattern"/> in front of <paramref name="name"/>, never twice.</summary>
        public string ApplyPattern(string name) => ApplyPattern(pattern, name);

        public static string NormalizePattern(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            value = value.Trim();
            if (value.EndsWith(':') || value.EndsWith('_') || value.EndsWith('-') || value.EndsWith('.'))
                return value;
            return value + ":";
        }

        public static string ApplyPattern(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name ?? "";
            var prefix = NormalizePattern(value);
            if (prefix.Length == 0 || name.StartsWith(prefix, StringComparison.Ordinal))
                return name;
            return prefix + name;
        }

        public bool IsSslEnabled =>
            string.Equals(sslEnabled, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sslEnabled, "1", StringComparison.OrdinalIgnoreCase);

        public string GetVirtualHost() =>
            string.IsNullOrWhiteSpace(virtualHost) ? "/" : virtualHost.Trim();

        public string GetConnectionString_SQL()
        {
            if (string.IsNullOrEmpty(port))
                return $"Data Source={host};" +
                       $"User ID={user};" +
                       $"Initial Catalog={dbName};" +
                       $"Password={pass};" +
                       $"Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;";

            return $"Data Source={host},{port};" +
                   $"User ID={user};" +
                   $"Initial Catalog={dbName};" +
                   $"Password={pass};" +
                   $"Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;";
        }

        /// <summary>
        /// TLS when <see cref="IsSslEnabled"/> or <see cref="caPath"/> is non-empty.
        /// Empty sslEnabled + empty caPath → plaintext.
        /// </summary>
        public bool UseTls =>
            IsSslEnabled || !string.IsNullOrWhiteSpace(caPath);

        /// <summary>Alias of <see cref="UseTls"/> for Redis callers.</summary>
        public bool UseRedisTls => UseTls;

        /// <summary>
        /// Effective AMQP port: user-set <see cref="port"/> if present;
        /// otherwise 5671 when <see cref="UseTls"/>, else 5672.
        /// </summary>
        public string GetRabbitMqPort() =>
            string.IsNullOrWhiteSpace(port)
                ? (UseTls ? "5671" : "5672")
                : port.Trim();

        public string GetConnectionString_REDIS()
        {
            var sb = new StringBuilder();
            sb.Append(host).Append(':').Append(string.IsNullOrEmpty(port) ? "6379" : port);

            if (!string.IsNullOrWhiteSpace(user))
                sb.Append(",user=").Append(user);
            if (!string.IsNullOrWhiteSpace(pass))
                sb.Append(",password=").Append(pass);

            if (UseTls)
            {
                sb.Append(",ssl=true");
                sb.Append(",sslHost=").Append(host);
            }

            return sb.ToString();
        }

        /// <summary>Log-safe summary — never includes password.</summary>
        public string GetSafeSummary()
        {
            return $"type={connectionType}; host={host}; port={port}; user={user}; " +
                   $"dbName={dbName}; ssl={UseTls}; vhost={GetVirtualHost()}; caPath={caPath}; pattern={pattern}";
        }

        public string GetConnectionString_RabbitMQ()
        {
            var scheme = UseTls ? "amqps" : "amqp";
            var effectivePort = GetRabbitMqPort();
            var vh = GetVirtualHost();
            var path = vh == "/" ? "" : Uri.EscapeDataString(vh.TrimStart('/'));

            // Prefer ConnectionFactory; this string is mainly for logging (password redacted).
            return $"{scheme}://{user}:***@{host}:{effectivePort}/{path}";
        }

        public string GetConnectionString_Postgre()
        { return $"User ID={user};Password={pass};Host={host};Port={port};Database={dbName};Pooling=true;Include Error Detail=true"; }

        /// <summary>
        /// Dynamically Return Connection String
        /// </summary>
        /// <returns></returns>
        public void InheritConnectionTypeFrom(GeneralConnectionString source)
        {
            if (string.IsNullOrWhiteSpace(connectionType) && !string.IsNullOrWhiteSpace(source?.connectionType))
                connectionType = source.connectionType;
        }

        public string GetConnectionString()
        {
            switch ((connectionType ?? "").Trim().ToLowerInvariant())
            {
                case "sql":
                case "sqlserver":
                    return GetConnectionString_SQL();
                case "redis":
                    return GetConnectionString_REDIS();
                case "postgress":
                case "postgres":
                case "postgresql":
                    return GetConnectionString_Postgre();
                case "rabbitmq":
                    return GetConnectionString_RabbitMQ();
                default:
                    return "";
            }
        }
    }
}
