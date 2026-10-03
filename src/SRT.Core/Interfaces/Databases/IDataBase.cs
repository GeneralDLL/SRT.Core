using Microsoft.AspNetCore.Builder;
using SRT.Core.Domain;

namespace SRT.Core.Interfaces.Databases
{
    public enum DatabaseProvider
    {
        SqlServer,
        PostgreSql
    }

    /// <summary>
    /// Database context contract used by host startup discovery.
    /// Lifecycle per context (in order): register via <see cref="BaseConfiguration"/>,
    /// migrate, <see cref="InsertBaseData"/>, <see cref="ApplyStartupDataMaintenanceAsync"/>,
    /// then provider-specific optional maintenance (e.g. SQL Server identity counter fix when enabled).
    /// </summary>
    public interface IDataBase
    {
        DatabaseProvider Provider { get; }

        void BaseConfiguration(WebApplicationBuilder builder, AppConnectionString ConnectionStrings);

        Task InsertBaseData();

        Task ApplyStartupDataMaintenanceAsync();
    }
}
