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
    }
}
