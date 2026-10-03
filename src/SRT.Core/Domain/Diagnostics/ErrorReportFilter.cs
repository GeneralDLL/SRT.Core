namespace SRT.Core.Domain.Diagnostics
{
    /// <summary>
    /// Filter for ErrorReport search/delete. Empty strings are treated as unset by the repository.
    /// Skip/Take apply only to search.
    /// </summary>
    public sealed class ErrorReportFilter
    {
        public Guid? ErrorId { get; set; }
        public string? CorrelationId { get; set; }
        public string? Category { get; set; }
        public string? Service { get; set; }
        public string? ServiceName { get; set; }
        public string? Environment { get; set; }
        public string? Operation { get; set; }
        public string? Severity { get; set; }
        public string? Outcome { get; set; }
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public int? ServiceId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? MessageContains { get; set; }

        /// <summary>Search only.</summary>
        public int Skip { get; set; }

        /// <summary>Search only. Default 50; repository clamps to max 200.</summary>
        public int Take { get; set; } = 50;
    }
}
