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

        public GeneralConnectionString GeneralDLL_CacheRedis { get; set; } = new();
        public GeneralConnectionString GeneralDLL_CacheSQL { get; set; } = new();
        public GeneralConnectionString GeneralDLL_ErrorSQL { get; set; } = new();
        public GeneralConnectionString GeneralDLL_RabbitMQ { get; set; } = new();

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

        public string GetConnectionString_REDIS()
        {
            var sb = new StringBuilder();
            sb.Append(host).Append(':').Append(string.IsNullOrEmpty(port) ? "6379" : port);

            if (!string.IsNullOrWhiteSpace(user))
                sb.Append(",user=").Append(user);
            if (!string.IsNullOrWhiteSpace(pass))
                sb.Append(",password=").Append(pass);

            if (IsSslEnabled)
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
                   $"dbName={dbName}; ssl={IsSslEnabled}; vhost={GetVirtualHost()}; caPath={caPath}";
        }

        public string GetConnectionString_RabbitMQ()
        {
            var scheme = IsSslEnabled ? "amqps" : "amqp";
            var vhost = Uri.EscapeDataString(GetVirtualHost().TrimStart('/'));
            if (GetVirtualHost() == "/")
                vhost = "";
            else if (GetVirtualHost().StartsWith("/"))
                vhost = Uri.EscapeDataString(GetVirtualHost());

            // Prefer ConnectionFactory; this string is mainly for logging (password redacted via GetSafeSummary).
            return $"{scheme}://{user}:***@{host}:{port}/{GetVirtualHost().TrimStart('/')}";
        }

        public string GetConnectionString_Postgre()
        { return $"User ID={user};Password={pass};Host={host};Port={port};Database={dbName};Pooling=true;Include Error Detail=true"; }

        /// <summary>
        /// Dynamically Return Connection String
        /// </summary>
        /// <returns></returns>
        public string GetConnectionString()
        {
            switch (connectionType.ToLower())
            {
                case "sql":
                    return GetConnectionString_SQL();
                case "redis":
                    return GetConnectionString_REDIS();
                case "postgress":
                    return GetConnectionString_Postgre();
                case "rabbitmq":
                    return GetConnectionString_RabbitMQ();
                default:
                    return "";
            }
        }
    }
}
