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
        public string? Category { get; set; }
        public string? Severity { get; set; }
        public string? Outcome { get; set; }
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public string? CorrelationId { get; set; }
        public string? Message { get; set; }
    }
}
