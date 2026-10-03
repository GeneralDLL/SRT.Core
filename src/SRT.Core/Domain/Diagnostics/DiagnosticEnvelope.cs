using System.Text.Json;
using System.Text.Json.Serialization;

namespace SRT.Core.Domain.Diagnostics
{
    public static class DiagnosticCategories
    {
        public const string Http = "Http";
        public const string SqlMigrate = "SqlMigrate";
        public const string Redis = "Redis";
        public const string RabbitConnect = "RabbitConnect";
        public const string RabbitConsume = "RabbitConsume";
        public const string RabbitPublish = "RabbitPublish";
        public const string Outbox = "Outbox";
    }

    public static class DiagnosticOutcomes
    {
        public const string Failed = "Failed";
        public const string Retrying = "Retrying";
        public const string Dead = "Dead";
        public const string Recovered = "Recovered";
        public const string Exhausted = "Exhausted";
    }

    public class DiagnosticDetail
    {
        public bool? Ssl { get; set; }
        public string? VirtualHost { get; set; }
        public string? Queue { get; set; }
        public string? RoutingKey { get; set; }
        public string? MessageId { get; set; }
        public long? Attempt { get; set; }
        public int? MaxAttempts { get; set; }
        public string? DlqName { get; set; }
        public string? PendingMigrations { get; set; }
        public string? DbContext { get; set; }
    }

    public class DiagnosticEnvelope
    {
        public Guid ErrorId { get; set; } = Guid.NewGuid();
        public string? CorrelationId { get; set; }
        public string Service { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        public string? Environment { get; set; }
        public string? Version { get; set; }
        public string Category { get; set; } = "";
        public string Operation { get; set; } = "";
        public string Severity { get; set; } = "Error";
        public string Outcome { get; set; } = DiagnosticOutcomes.Failed;
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public DiagnosticDetail Detail { get; set; } = new();

        [JsonIgnore]
        public bool PersistToErrorDb { get; set; } = true;

        public string ToDetailJson() =>
            JsonSerializer.Serialize(Detail, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            });
    }
}
