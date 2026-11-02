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
6. `AddApiVersioning` (`ReportApiVersions = true`, default `1.0`, `AssumeDefaultVersionWhenUnspecified = true`) with `.AddMvc().AddApiExplorer()`
7. `config.lstInjectService.Add(AddAuthentication)`

### `SRT_ConfigBuilderAndBuildApp`

Controllers (`ReturnHttpNotAcceptable` default false) + `LocalDateTimeJsonConverter` + `ApiBehaviorOptions.InvalidModelStateResponseFactory` (`ResponseData` 400, not `ValidationProblemDetails`) + `AddEndpointsApiExplorer` → run `lstInjectService` with section `BaseURLtagName` (default `"urlData"`) → load plugins → `ValidateRequestedFeatures` → each plugin `SRT_ConfigBuilder` by Order → `lstBuilderEnvironments` → `[InjectionControllerAttribute]` scan → singleton `AppSetting`, `ISystemENV`, `AppConnectionString`, service status → `Build()` → `UseServiceStatus` (GET `/`) → `HttpOnlyMiddleWareAttribute` → `lstAppEnvironments` → each plugin `SRT_ConfigApp` → **`MapControllers`**.

`UseAuthorization()` is **commented out** in Core. Hosts still must not add it.

`SrtPluginLoader` is **internal**.

Validation throws `InvalidOperationException` if a requested feature has no plugin, or required `host` is empty (`AllowCacheSqlFallback` skips cache SQL/Postgres host check).

---

### Startup summary — `SRTStartupReport` (`SRT.Core.Domain.Diagnostics`)

Startup writes ONE log record, category `SRT.Startup`, event id 1000. Success is `Warning` (passes a Production `Default: Warning` filter), a FAILED part or a startup exception is `Error` with the exception attached. It goes through `ILogger`, so Serilog and OpenTelemetry both get it. Every part is also an attribute: `srt.startup.<section>.status` / `.detail`, plus `srt.startup.outcome|service|environment|elapsed_ms|urls`.

- `configApp.StartupReport` is the instance. It is also registered in DI as a singleton.
- `Set(section, status, detail)` adds or replaces a part. Helpers: `Ok`, `Off`, `Failed`, `Info`, `Append`. Status values: `OK`, `OFF`, `FAILED`, `SKIPPED`, `INFO`.
- SRT.Core fills `plugins` and `grpc`. Plugins fill `sql.<DbContext>` (target, migrations, seed, maintenance, identity gap), `redis` (with ping), `rabbitmq` (with a connect probe, not fatal), `swagger`, `telemetry`, `serilog`. A service can add its own part from `SRT_ConfigBuilder` / `SRT_ConfigApp`.
- Written once: on `ApplicationStarted` (with the real URLs), or at the first plugin exception. Never put a password or a raw connection string in a detail.
- The old per-step `LogWarning("... - OK")` lines are now `Debug`.

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
| `pattern` | Per-service name prefix for Redis keys (`SRTCore_CacheRedis`) and RabbitMQ queue/exchange names (`SRTCore_RabbitMQ`); empty = no prefix |
| `NormalizedPattern` / `NormalizePattern(value)` | Trim; append `:` unless it ends with `:`, `_`, `-` or `.`; empty stays empty |
| `ApplyPattern(name)` / `ApplyPattern(value, name)` | Prepends the normalized pattern unless `name` is empty or already starts with it |
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

`IErrorLogger` (`SRT.Core.Diagnostics`): Save_Error_ToDB, Save_Error_WithEnvelope, Save_Log, GetErrorString, SearchByFilterAsync, DeleteByFilterAsync. Filter Take default 50. Viewer methods SearchPageAsync, GetByIdAsync, GetByCorrelationAsync, FacetsAsync, StatsAsync, ExportCsvAsync, PurgeOlderThanAsync have a default implementation that throws `NotSupportedException`; the SQL Server store overrides them.

`HttpDiagnosticEnvelope.Create(HttpContext, statusCode)` (`SRT.Core.Domain.Diagnostics`) builds the Http envelope that `SaveLogError` and `CheckAuthMiddleWareAttribute` store. Detail holds method, path, query string, route, status, user id, role, active role, client IP, user agent, referer, flow trace id and request headers. `Authorization`, `Cookie`, `Proxy-Authorization` and `X-Api-Key` are dropped. The request body is never stored.

`TypeAuthenticationController.DevOps` matches `UserRoleType.DevOps`.

`IDataBase`: `Provider` (`SqlServer` \| `PostgreSql`), `BaseConfiguration`, `InsertBaseData`, `ApplyStartupDataMaintenanceAsync`.

`IUnitOfWork`: `SaveChangesAsync`, `ExecuteInTransactionAsync`.

`IRepository<TEntity,TKey>`: GetById, AddOrUpdate (`autoSave: true` saves **all** pending changes), AddOrUpdateRange, DeleteByFilter (immediate `ExecuteDelete` at implementer).

---

## 6. API controllers — `SRTBaseControllerSystem` (`SRT.Core.SYS_Controllers`)

Consumer API controllers inherit this type. It is `[ApiController]`, `abstract`, `ControllerBase`, and `IActionFilter`. There is **no** `[Route]` on the base. Helpers are `[NonAction]`.

```csharp
[ApiController]
public abstract class SRTBaseControllerSystem : ControllerBase, IActionFilter
```

Parameterless constructor. Services resolve **lazily** from `HttpContext.RequestServices` when accessed (not in the constructor).

| Member | Resolve |
|--------|---------|
| `Logger` | `ILoggerFactory.CreateLogger(GetType())` (derived type name) |
| `SRT_Base_AppConfiguration` | `GetRequiredService<IConfiguration>()` |
| `SRT_Base_AppSetting` | `GetRequiredService<AppSetting>()` |
| `SRT_Base_AppConnectionString` | `GetRequiredService<AppConnectionString>()` |
| `SRT_Base_SystemENV` | `GetRequiredService<ISystemENV>()` |
| `SRT_Base_ErrorLogger` | `GetService<IErrorLogger>()` — **null** when error-log SQL/Postgres is not registered |

Headers (read in `OnActionExecuting`; invalid values log a warning and keep defaults; they are **not** authorization):

- `X-Device-Identity` JSON object → `headerBrowser`, `headerBrowserID`
- `X-Role-Identity` GUID → `headerUserRoleGUID` (`Guid.Empty` if missing/invalid)

User: `GetUserInfo()` / `dmUser` / `dmAdmin` return `IUserInfo?` from `HttpContext.Items["UserData"]` (same key as `CheckAuthMiddleWareAttribute`). Missing user is `null`, not 500.

`IUserInfo` exposes `isSuperAdmin` and `roleGrants`. Each `UserRoleGrant` has `code` and that role's `accesses`. Missing or null JSON for those members means `false` and an empty list. An old token whose `roles` value is a string array is read as grants with empty `accesses`; a flat `accesses` list is ignored. `CheckAuthMiddleWareAttribute` takes optional `requiredAccess`. After a valid token, a superadmin skips the controller role check and the active-role check. Other users still match `UserRoleType`. When `requiredAccess` is set, the request must send exactly one `X-Active-Role` header. That code is compared with `StringComparer.Ordinal` to `roleGrants`, and the key must be in that role only. Empty, repeated, or comma-joined header values are HTTP 403 `ACTIVE_ROLE_REQUIRED`. An unknown code is `ACTIVE_ROLE_UNKNOWN`. A missing key is `FORBIDDEN_ACCESS`. The accepted code is stored only in `HttpContext.Items["ActiveRole"]` for that request. An invalid token stays HTTP 401. Empty `requiredAccess` does not read the header. `X-Role-Identity` remains a GUID and is not this check.

Envelope type: `SRT.Core.Domain.RequestsDomqins.ResponseData` (`status`, `data`, `message`, `errorCode`, `redirectURL`). Host JSON formatter is System.Text.Json (`AddJsonOptions`). Do not stringify the whole payload.

```csharp
public static ObjectResult CreateApiResponse(
    HttpStatusCode status,
    string? message = null,
    object? data = null,
    string? errorCode = null,
    string? redirectUrl = null)
```

HTTP status on the result equals `status`. Helper map:

| Method | HTTP |
|--------|------|
| `Response_OK` | 200 |
| `Response_NoContent` | 204 (no `ResponseData` body) |
| `Response_Error_BadRequest` / `Response_Error_InputParameterError` / `Response_Error_NotResponseFromChild` | 400 |
| `Response_Error_Unauthorized` | 401 |
| `Response_Error_NotFoundInputError` | 404 |
| `Response_Error_ExistedDataBefore` (aliases `FinishedProcessBefore`, `FoundDataBefore`) | 409 |
| `Response_Error_TooManyRequests` | 429 |
| `Response_Error_ControlInternalServerError` | 500 (logs via `IErrorLogger` when registered) |
| `Response_Error_ServiceUnavailable` | 503 |
| `Response_Error_GatewayTimeout` | 504 |

`Response_GenerateResult(ResponseData data)` switches on `data.status` for the rows above. Unknown status → `InvalidOperationException`. File/stream results are not wrapped.

Automatic 400 (before the action): `InvalidModelStateResponseFactory` returns `CreateApiResponse(BadRequest, ...)`.
