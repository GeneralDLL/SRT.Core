namespace SRT.Core.Domain.Diagnostics
{
    /// <summary>
    /// Filter for ErrorReport search/delete. Empty strings are treated as unset by the repository.
    /// Skip/Take apply only to SearchByFilterAsync; Page/PageSize/SortBy/SortDir only to the paged viewer search.
    /// </summary>
    public sealed class ErrorReportFilter
    {
        public long? DbId { get; set; }
        public Guid? ErrorId { get; set; }
        public string? CorrelationId { get; set; }
        public string? Category { get; set; }
        public string? LegacyCategory { get; set; }
        public string? Service { get; set; }
        public string? ServiceName { get; set; }
        public string? Environment { get; set; }
        public string? Version { get; set; }
        public string? Operation { get; set; }
        public string? OperationContains { get; set; }
        public string? Severity { get; set; }
        public string? Outcome { get; set; }
        public string? ExceptionType { get; set; }
        public string? Target { get; set; }
        public string? TargetContains { get; set; }
        public int? ServiceId { get; set; }

        /// <summary>Last N hours on the server clock. Wins over DateFrom/DateTo, because the browser clock may differ from the server.</summary>
        public int? LastHours { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? MessageContains { get; set; }
        public string? StackTraceContains { get; set; }

        /// <summary>Search only.</summary>
        public int Skip { get; set; }

        /// <summary>Search only. Default 50; repository clamps to max 200.</summary>
        public int Take { get; set; } = 50;

        /// <summary>Paged search only. 1-based.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Paged search only. Default 20; repository clamps to max 100.</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>Paged search only: date (default) or severity.</summary>
        public string? SortBy { get; set; }

        /// <summary>Paged search only: desc (default) or asc.</summary>
        public string? SortDir { get; set; }
    }
}
