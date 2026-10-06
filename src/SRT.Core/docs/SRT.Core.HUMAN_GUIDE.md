# SRT.Core — Human Guide

This guide explains the SRT host pipeline, plugins, and configuration. Exact signatures: [SRT.Core.API_REFERENCE.md](SRT.Core.API_REFERENCE.md). Recipes: [SRT.Core.COOKBOOK.md](SRT.Core.COOKBOOK.md).

## What this package is

`SRT.Core` is the shared host, `AppSetting`, connection-string types, plugin contract, and data/cache/error interfaces. Feature plugins live in other `SRT.Core.*` assemblies loaded from the host output directory (`SRT.Core*.dll`).

It does **not** implement SQL, Redis, RabbitMQ, Serilog, or Swagger. Those are separate packages. If `GetRequestedFeatures()` lists a feature and the plugin DLL is missing, startup throws.

## How a web host starts

```csharp
var dmAppSetting = args.SRT_ConfigController();
var (builder, config) = StartAppConfigurations.SRT_ConfigController(args, dmAppSetting);
var app = await builder.SRT_ConfigBuilder(config)
    .SRT_ConfigBuilderAndBuildApp(config)
    .SRT_ConfigApp(config);
app.Run();
```

| Step | Where | Role |
|------|--------|------|
| `args.SRT_ConfigController()` | Host `BasicProjectConfig` | Build `MyAppSetting` |
| `StartAppConfigurations.SRT_ConfigController` | Host GeneralDLL | Call Core `BaseConfigurations.SRT_ConfigController` with an `AddAuthentication` delegate |
| `builder.SRT_ConfigBuilder` | Host | **Local DI only** |
| `SRT_ConfigBuilderAndBuildApp` | **Core** | Controllers, plugins, build, Core middleware, `MapControllers` |
| `SRT_ConfigApp` | Host (`Task` then `WebApplication` overloads) | **Local middleware only** |

Do not put Swagger, SQL, or Core middleware in `Program.cs`. Do not call `UseAuthorization` or `MapControllers` in the host (`MapControllers` is in Core; `UseAuthorization` is currently commented out there).

## API controllers

JSON API controllers inherit `SRT.Core.SYS_Controllers.SRTBaseControllerSystem` (`[ApiController]`, `ControllerBase`). Helpers return `ResponseData` with a matching HTTP status. Core sets `InvalidModelStateResponseFactory` so automatic 400s use the same envelope.

`CheckAuthMiddleWareAttribute` writes `IUserInfo` to `HttpContext.Items["UserData"]`. Controllers read it with `GetUserInfo()`. `X-Role-Identity` is a device/role header on the base controller, not authorization.

Do not inherit `Controller` / `ControllerBase` directly in consumer APIs. Do not invent a second envelope type.

## Plugins

Internal `SrtPluginLoader` loads `SRT.Core*.dll` from `AppContext.BaseDirectory`, instantiates public non-abstract `ISRTBasicConfigurations` with a parameterless constructor, then runs them by `Order` then `FeatureName`.

Known orders: Logs **10**, SQL Server **20**, PostgreSQL **21**, Redis **30**, RabbitMQ **40**, Swagger **50**.

`AppSetting.GetRequestedFeatures()` decides which plugins must be present:

| Condition | Feature |
|-----------|---------|
| `UseSerilog` (default true) | `Logs` |
| `SwaggerConfig` property non-null | `Swagger` |
| ErrorLog/cache SQL or SQL app DBs | `SQLServer` |
| ErrorLog/cache Postgres or Postgres app DBs | `PostgreSQL` |
| `UseCashStructure != None` | `Redis` |
| `lstRabbitMQNamespace` non-empty | `RabbitMQ` |

## Store routing

Flags `UseErrorLogSQL` and `UseCashStructure` are **not** enough. Core also looks at `connectionType` (empty `connectionType` = SQL Server):

- `IsErrorLogSqlStore()` / `IsErrorLogPostgresStore()`
- `IsCacheSqlStore()` / `IsCachePostgresStore()`
- `IsSqlDatabaseRequested()` / `IsPostgresDatabaseRequested()`

`SRTCore_ErrorSQL` / `SRTCore_CacheSQL` inherit empty `connectionType` from `DefaultConnection` during `SRT_ConfigController`.

## Connection slots (`ConnectionDB`)

`DefaultConnection`, `DefaultConnection2`, `SRTCore_CacheRedis`, `SRTCore_CacheSQL`, `SRTCore_ErrorSQL`, `SRTCore_RabbitMQ`.

TLS for Redis/Rabbit: `UseTls` = `sslEnabled` true/1 **or** non-empty `caPath`. Empty both → plaintext.

Service identity: bind `Service:Id` / `Service:Name` (or `ServiceId` / `ServiceName`) in every environment appsettings file.
