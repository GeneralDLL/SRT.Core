namespace SRT.Core.Domain.Diagnostics
{
    /// <summary>
    /// Flat list projection for ErrorReport search results (no chunk columns).
    /// </summary>
    public sealed class ErrorReportSummary
    {
        public long Id { get; set; }
        public Guid? ErrorId { get; set; }
        public DateTime? Date { get; set; }
        public string? ServiceName { get; set; }
        public string? Service { get; set; }
        public string? Environment { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public string? Outcome { get; set; }
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public string? CorrelationId { get; set; }
        public string? Message { get; set; }
    }

    public sealed class ErrorReportPage
    {
        public IReadOnlyList<ErrorReportSummary> Items { get; set; } = [];
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>One ErrorReport row with every column. Detail is DetailJson parsed, or null when it is not valid JSON.</summary>
    public sealed class ErrorReportDetail
    {
        public long Id { get; set; }
        public Guid? ErrorId { get; set; }
        public DateTime? Date { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public string? Service { get; set; }
        public string? Environment { get; set; }
        public string? Version { get; set; }
        public string? Category { get; set; }
        public string? LegacyCategory { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public string? Outcome { get; set; }
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public string? CorrelationId { get; set; }
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public IReadOnlyList<string> ExceptionChain { get; set; } = [];
        public string? DetailJson { get; set; }
        public object? Detail { get; set; }
    }

    public sealed class ErrorReportFacetValue
    {
        public string Value { get; set; } = "";
        public int Count { get; set; }
    }

    public sealed class ErrorReportFacets
    {
        public IReadOnlyList<ErrorReportFacetValue> Severity { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> Category { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> Outcome { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> ExceptionType { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> Environment { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> Version { get; set; } = [];
        public IReadOnlyList<ErrorReportFacetValue> Operation { get; set; } = [];
    }

    public sealed class ErrorReportStatsBucket
    {
        public DateTime Start { get; set; }
        public int Total { get; set; }
        public IReadOnlyDictionary<string, int> BySeverity { get; set; } = new Dictionary<string, int>();
    }

    public sealed class ErrorReportStats
    {
        /// <summary>hour when the range is 48 hours or less, otherwise day.</summary>
        public string Bucket { get; set; } = "hour";
        public IReadOnlyList<ErrorReportStatsBucket> Items { get; set; } = [];
    }
}
