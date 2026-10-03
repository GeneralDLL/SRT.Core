using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SRT.Core.Diagnostics;

namespace SRT.Core.Domain.Diagnostics
{
    public static class DiagnosticReporter
    {
        public static async Task ReportAsync(
            Exception exp,
            DiagnosticEnvelope envelope,
            ILogger logger,
            IServiceScopeFactory? scopeFactory = null)
        {
            envelope.ExceptionType ??= exp.GetType().FullName;
            envelope.Environment ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            envelope.Service = string.IsNullOrWhiteSpace(envelope.Service)
                ? AppDomain.CurrentDomain.FriendlyName
                : envelope.Service;

            if (scopeFactory is not null)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var errorLogger = scope.ServiceProvider.GetService<IErrorLogger>();
                    if (errorLogger is not null)
                    {
                        await errorLogger.Save_Error_WithEnvelope(exp, envelope, envelope.PersistToErrorDb)
                            .ConfigureAwait(false);
                        return;
                    }
                }
                catch (Exception persistEx)
                {
                    logger.LogCritical(persistEx, "Error persist failed for ErrorId={ErrorId}", envelope.ErrorId);
                }
            }

            logger.LogError(
                exp,
                "Diag ErrorId={ErrorId} Category={Category} Operation={Operation} Outcome={Outcome} Target={Target} CorrelationId={CorrelationId} Service={Service} Detail={Detail}",
                envelope.ErrorId,
                envelope.Category,
                envelope.Operation,
                envelope.Outcome,
                envelope.Target,
                envelope.CorrelationId,
                envelope.Service,
                envelope.ToDetailJson());
        }
    }
}
