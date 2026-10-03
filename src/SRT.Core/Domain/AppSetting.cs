using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SRT.Core.ENV;
using SRT.Core.Interfaces;
using SRT.Core.SystemEnums;

namespace SRT.Core.Domain
{
    public class AppSetting
    {
        #region Constructors
#pragma warning disable CS8618
        public AppSetting(List<string>? lstDatabaseNamespace = null)
        {
            AddResponseResult(StatusCodes.Status500InternalServerError, typeof(void));
            this.lstDatabaseNamespace = lstDatabaseNamespace ?? new List<string>();
        }
#pragma warning restore CS8618

        public AppSetting(IWebHostEnvironment _env, AppSetting _base)
        {
            CoreSystemENV = new SystemENV(_env);
            lstResponseResult = _base.lstResponseResult;
            ReturnHttpNotAcceptable = _base.ReturnHttpNotAcceptable;
            UseCashStructure = _base.UseCashStructure;
            UseErrorLogSQL = _base.UseErrorLogSQL;
            UseSerilog = _base.UseSerilog;
            AllowCacheSqlFallback = _base.AllowCacheSqlFallback;
            ServiceId = _base.ServiceId;
            ServiceName = _base.ServiceName;
            ConnectionStrings = _base.ConnectionStrings;
            lstBuilderEnvironments = _base.lstBuilderEnvironments;
            lstAppEnvironments = _base.lstAppEnvironments;
            lstControllersOptions = _base.lstControllersOptions;
            lstDatabaseNamespace = _base.lstDatabaseNamespace;
            lstRabbitMQNamespace = _base.lstRabbitMQNamespace;
            lstInjectService = _base.lstInjectService;
            lstInjection_WithInjectionAttribute = _base.lstInjection_WithInjectionAttribute;
            BaseURLtagName = _base.BaseURLtagName;
        }
        #endregion

        public void AddResponseResult(int statusCode, Type type)
        {
            lstResponseResult.Add(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(statusCode: statusCode, type: type));
        }

        public List<Action<Microsoft.AspNetCore.Mvc.MvcOptions>> lstControllersOptions { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute> lstResponseResult { get; set; } = new();
        public bool ReturnHttpNotAcceptable { get; set; } = false;
        public bool UseErrorLogSQL { get; set; } = true;
        public bool UseSerilog { get; set; } = true;
        public CashType UseCashStructure { get; set; } = CashType.CashWithSQL;
        public bool AllowCacheSqlFallback { get; set; } = false;

        /// <summary>
        /// Per-microservice id from appsettings <c>Service:Id</c> (or <c>ServiceId</c>).
        /// Used to isolate SQL cache rows (<c>CashBackup.serviceID</c>). 0 = general.
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Per-microservice name from appsettings <c>Service:Name</c> (or <c>ServiceName</c>).
        /// Empty falls back to the process friendly name at write time.
        /// </summary>
        public string ServiceName { get; set; } = AppDomain.CurrentDomain.FriendlyName;

        public string BaseURLtagName { get; set; } = "urlData";

        public AppConnectionString ConnectionStrings { get; set; }
        public SystemENV CoreSystemENV { get; set; }

        public List<Action<WebApplicationBuilder, SystemENV.EnvironmentType>> lstBuilderEnvironments { get; set; } = new();
        public List<Action<WebApplication, SystemENV.EnvironmentType>> lstAppEnvironments { get; set; } = new();

        public List<Func<IServiceCollection, URLData, IServiceCollection>> lstInjectService { get; set; } = new();

        public List<string> lstDatabaseNamespace { get; set; } = new();
        public List<string> lstRabbitMQNamespace { get; set; } = new();

        public List<string> lstInjection_WithInjectionAttribute { get; set; } = new();

        public bool HasSwaggerConfig =>
            GetType().GetProperty("SwaggerConfig")?.GetValue(this) != null;

        public IReadOnlyList<string> GetRequestedFeatures()
        {
            var list = new List<string>();
            if (UseSerilog)
                list.Add(SRTFeatureNames.Logs);
            if (HasSwaggerConfig)
                list.Add(SRTFeatureNames.Swagger);
            if (IsErrorLogSqlStore() || IsCacheSqlStore() || IsSqlDatabaseRequested())
                list.Add(SRTFeatureNames.SQLServer);
            if (IsErrorLogPostgresStore() || IsCachePostgresStore() || IsPostgresDatabaseRequested())
                list.Add(SRTFeatureNames.PostgreSQL);
            if (UseCashStructure != CashType.None)
                list.Add(SRTFeatureNames.Redis);
            if (lstRabbitMQNamespace.Count > 0)
                list.Add(SRTFeatureNames.RabbitMQ);
            return list;
        }

        public bool IsSqlDatabaseRequested()
        {
            if (lstDatabaseNamespace == null || lstDatabaseNamespace.Count == 0)
                return false;
            return IsSqlServerConnectionType(ConnectionStrings?.DefaultConnection?.connectionType);
        }

        public bool IsPostgresDatabaseRequested()
        {
            if (lstDatabaseNamespace == null || lstDatabaseNamespace.Count == 0)
                return false;
            return IsPostgresConnectionType(ConnectionStrings?.DefaultConnection?.connectionType);
        }

        public bool IsErrorLogSqlStore()
            => UseErrorLogSQL && IsSqlServerConnectionType(ResolveRelationalStoreType(ConnectionStrings?.SRTCore_ErrorSQL));

        public bool IsErrorLogPostgresStore()
            => UseErrorLogSQL && IsPostgresConnectionType(ResolveRelationalStoreType(ConnectionStrings?.SRTCore_ErrorSQL));

        public bool IsCacheSqlStore()
            => UseCashStructure == CashType.CashWithSQL
               && IsSqlServerConnectionType(ResolveRelationalStoreType(ConnectionStrings?.SRTCore_CacheSQL));

        public bool IsCachePostgresStore()
            => UseCashStructure == CashType.CashWithSQL
               && IsPostgresConnectionType(ResolveRelationalStoreType(ConnectionStrings?.SRTCore_CacheSQL));

        public string ResolveRelationalStoreType(GeneralConnectionString? store)
        {
            var type = store?.connectionType?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(type))
                return type;
            return ConnectionStrings?.DefaultConnection?.connectionType?.Trim() ?? "";
        }

        public static bool IsPostgresConnectionType(string? connectionType)
        {
            var type = connectionType?.Trim().ToLowerInvariant() ?? "";
            return type is "postgress" or "postgres" or "postgresql";
        }

        public static bool IsSqlServerConnectionType(string? connectionType)
        {
            var type = connectionType?.Trim().ToLowerInvariant() ?? "";
            return string.IsNullOrWhiteSpace(type) || type is "sql" or "sqlserver";
        }
    }
}
