# SRT.Core — API Reference

Reviewed: Core repo `95b0a5b`.  
Prefer this document for consumption. If docs disagree with code, inspect the related source and update this file.

---

## 1. Host — `BaseConfigurations` (`SRT.Core`)

```csharp
public static (WebApplicationBuilder, TAppSetting) SRT_ConfigController<TAppSetting>(
    string[] args, TAppSetting config,
    Func<IServiceCollection, URLData, IServiceCollection> AddAuthentication)
    where TAppSetting : AppSetting

public static async Task<WebApplication> SRT_ConfigBuilderAndBuildApp(
    WebApplicationBuilder builder, AppSetting configApp)

public static ILogger SRT_GetLogger(this WebApplicationBuilder builder, string title = "configure Application")
public static ILogger SRT_GetLogger(this WebApplication app, string title = "configure Application")
```

### `SRT_ConfigController`

1. Environment: `args[0]` if present, else `ASPNETCORE_ENVIRONMENT`, else `"Development"` → `SystemENV.FixEnvironmentName`
2. `WebApplication.CreateBuilder` with that environment
3. `config.CoreSystemENV`
4. Bind `ConnectionDB` → `AppConnectionString`; empty Error/Cache SQL `connectionType` inherits from `DefaultConnection`
5. Bind `Service:Id` / `ServiceId`, `Service:Name` / `ServiceName`
6. `AddApiVersioning` (`ReportApiVersions = true`)
7. `config.lstInjectService.Add(AddAuthentication)`

### `SRT_ConfigBuilderAndBuildApp`

Controllers (`ReturnHttpNotAcceptable` default false) + `LocalDateTimeJsonConverter` + `AddEndpointsApiExplorer` → run `lstInjectService` with section `BaseURLtagName` (default `"urlData"`) → load plugins → `ValidateRequestedFeatures` → each plugin `SRT_ConfigBuilder` by Order → `lstBuilderEnvironments` → `[InjectionControllerAttribute]` scan → singleton `AppSetting`, `ISystemENV`, `AppConnectionString`, service status → `Build()` → `UseServiceStatus` (GET `/`) → `HttpOnlyMiddleWareAttribute` → `lstAppEnvironments` → each plugin `SRT_ConfigApp` → **`MapControllers`**.

`UseAuthorization()` is **commented out** in Core. Hosts still must not add it.

`SrtPluginLoader` is **internal**.

Validation throws `InvalidOperationException` if a requested feature has no plugin, or required `host` is empty (`AllowCacheSqlFallback` skips cache SQL/Postgres host check).

---

## 2. Plugin contract

```csharp
public interface ISRTBasicConfigurations
{
    int Order { get; }
    string FeatureName { get; }
    Task<WebApplicationBuilder> SRT_ConfigBuilder(WebApplicationBuilder builder, AppSetting configApp);
    Task<WebApplication> SRT_ConfigApp(WebApplication app, AppSetting configApp);
}
```

`SRTFeatureNames`: `Logs`, `Swagger`, `SQLServer`, `PostgreSQL`, `Redis`, `RabbitMQ`.

---

## 3. `AppSetting` (`SRT.Core.Domain`)

Defaults: `ReturnHttpNotAcceptable = false`, `UseErrorLogSQL = true`, `UseSerilog = true`, `UseCashStructure = CashType.CashWithSQL`, `AllowCacheSqlFallback = false`, `ServiceId = 0`, `ServiceName` = process friendly name, `BaseURLtagName = "urlData"`.

`CashType`: `None`, `CashNoSQL`, `CashWithSQL`.

```csharp
IReadOnlyList<string> GetRequestedFeatures()
bool HasSwaggerConfig  // reflection: property "SwaggerConfig" value not null
bool IsSqlDatabaseRequested()
bool IsPostgresDatabaseRequested()
bool IsErrorLogSqlStore()    // UseErrorLogSQL && SQL type of ResolveRelationalStoreType(SRTCore_ErrorSQL)
bool IsErrorLogPostgresStore()
bool IsCacheSqlStore()       // CashWithSQL && SQL type of SRTCore_CacheSQL
bool IsCachePostgresStore()
string ResolveRelationalStoreType(GeneralConnectionString? store)
static bool IsPostgresConnectionType(string? connectionType)  // postgress|postgres|postgresql
static bool IsSqlServerConnectionType(string? connectionType) // empty OR sql|sqlserver
```

Empty `connectionType` is SQL Server.

---

## 4. `AppConnectionString` / `GeneralConnectionString`

Slots: `DefaultConnection`, `DefaultConnection2`, `SRTCore_CacheRedis`, `SRTCore_CacheSQL`, `SRTCore_ErrorSQL`, `SRTCore_RabbitMQ`.

| Member | Behavior |
|--------|----------|
| `IsSslEnabled` | `sslEnabled` is `"true"` or `"1"` (ignore case) |
| `UseTls` | `IsSslEnabled \|\| caPath` non-empty |
| `UseRedisTls` | Alias of `UseTls` |
| `GetVirtualHost()` | Empty → `"/"` |
| `GetRabbitMqPort()` | User `port` if set; else `5671` if `UseTls`, else `5672` |
| `GetConnectionString_SQL()` | `Encrypt=False`; `Trust Server Certificate=False`; timeout 30 |
| `GetConnectionString_REDIS()` | Default port 6379; `ssl=true,sslHost=host` when `UseTls` |
| `GetConnectionString_RabbitMQ()` | `amqps`/`amqp`; password redacted |
| `GetConnectionString_Postgre()` | Npgsql-style string |
| `GetConnectionString()` | By `connectionType`; unknown → `""` |
| `GetSafeSummary()` | No password |

---

## 5. Cache / error / data contracts

`ICacheRepository` (`SRT.Core.Interfaces.Cache`): AddNewData, AddPersistentData, GetData, RemoveData, ClearCategory, counters, rate-limit, backoff. `CacheServiceKeys.Sql = "sql"`. `ICacheSqlPersistence.IsAvailable`.

`IErrorLogger` (`SRT.Core.Diagnostics`): Save_Error_ToDB, Save_Error_WithEnvelope, Save_Log, GetErrorString, SearchByFilterAsync, DeleteByFilterAsync. Filter Take default 50.

`IDataBase`: `Provider` (`SqlServer` \| `PostgreSql`), `BaseConfiguration`, `InsertBaseData`, `ApplyStartupDataMaintenanceAsync`.

`IUnitOfWork`: `SaveChangesAsync`, `ExecuteInTransactionAsync`.

`IRepository<TEntity,TKey>`: GetById, AddOrUpdate (`autoSave: true` saves **all** pending changes), AddOrUpdateRange, DeleteByFilter (immediate `ExecuteDelete` at implementer).
