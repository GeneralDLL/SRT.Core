using SRT.Core.Domain.Diagnostics;

namespace SRT.Core.Diagnostics
{
    public interface IErrorLogger
    {
        Task<long> Save_Error_ToDB(Exception exp);
        Task<(Guid ErrorId, long DbId)> Save_Error_WithEnvelope(Exception exp, DiagnosticEnvelope envelope, bool persistToDb);
        Task<long> Save_Log(string text);
        string GetErrorString(Exception exp);

        Task<IReadOnlyList<ErrorReportSummary>> SearchByFilterAsync(ErrorReportFilter filter, CancellationToken ct = default);
        Task<int> DeleteByFilterAsync(ErrorReportFilter filter, CancellationToken ct = default);

        // shortcut: only the SQL Server store implements the viewer methods, add them to the Postgres store when it needs the viewer.
        Task<ErrorReportPage> SearchPageAsync(ErrorReportFilter filter, CancellationToken ct = default) => throw new NotSupportedException();
        Task<ErrorReportDetail?> GetByIdAsync(long id, CancellationToken ct = default) => throw new NotSupportedException();
        Task<IReadOnlyList<ErrorReportSummary>> GetByCorrelationAsync(string correlationId, CancellationToken ct = default) => throw new NotSupportedException();
        Task<ErrorReportFacets> FacetsAsync(ErrorReportFilter filter, CancellationToken ct = default) => throw new NotSupportedException();
        Task<ErrorReportStats> StatsAsync(ErrorReportFilter filter, CancellationToken ct = default) => throw new NotSupportedException();
        Task<byte[]> ExportCsvAsync(ErrorReportFilter filter, CancellationToken ct = default) => throw new NotSupportedException();
        Task<int> PurgeOlderThanAsync(int days, int batch, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
