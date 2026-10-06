# SRT.Core — AI Rules (canonical)

These rules are mandatory for any agent working in a repo that consumes or modifies **SRT.Core**.

Companion docs: [API_REFERENCE](SRT.Core.API_REFERENCE.md), [COOKBOOK](SRT.Core.COOKBOOK.md), [HUMAN_GUIDE](SRT.Core.HUMAN_GUIDE.md).

## Working rule

1. Read these docs first.
2. Do not invent APIs that duplicate `AppSetting`, `BaseConfigurations`, `SRTBaseControllerSystem`, `ResponseData`, `IRepository`, `IUnitOfWork`, `ICacheRepository`, or `IErrorLogger`.
3. If docs are ambiguous or wrong: inspect related code and update these docs.
4. Do not call internal `SrtPluginLoader`.

## Host

MUST:

- Keep `Program.cs` as the thin bootstrap in COOKBOOK.
- Put local DI in `BasicProjectConfig.SRT_ConfigBuilder`.
- Pass an `AddAuthentication` delegate into Core `SRT_ConfigController`.
- Project-reference every plugin `GetRequestedFeatures()` will request.
- Configure `ConnectionDB` hosts for enabled stores (see Core `ThrowIfIncomplete`).
- Bind `Service.Id` / `Service.Name` in every environment appsettings file.

MUST NOT:

- Call `UseAuthorization` or `MapControllers` in the host.
- Put Swagger / SQL / Redis / Rabbit / Serilog registration in `Program.cs`.
- Treat empty `connectionType` as PostgreSQL (it is SQL Server).
- Assume `UseErrorLogSQL` / `CashWithSQL` register SQL Server when `connectionType` is PostgreSQL.

## Controllers (consumer API hosts)

Applies to **API controllers in apps that consume SRT.Core**. Does **not** apply to `SRTBaseControllerSystem` itself, to MVC view controllers, or to file/stream endpoints.

MUST:

- Inherit `SRT.Core.SYS_Controllers.SRTBaseControllerSystem` (do not inherit `Controller` or `ControllerBase` directly).
- Return JSON bodies through `Response_OK` / `Response_Error_*` / `Response_GenerateResult` (`ResponseData` envelope). HTTP status must match `ResponseData.status`.
- Read the authenticated user from `GetUserInfo()` / `IUserInfo` (`HttpContext.Items["UserData"]`).
- Use `Response_NoContent()` for 204. File and stream results stay unwrapped.
- Put the route on the derived controller; do not add a shared `[Route]` on the base.

MUST NOT:

- Return raw `Ok(...)`, `NotFound()`, `StatusCode(...)`, or a parallel envelope for JSON.
- Use `UserInfo`, `GeneralDLL`, or `IGeneralDLLRepository`.
- Treat `X-Role-Identity` as authorization (`CheckAuthMiddleWareAttribute` is auth).
- Invent another base controller that duplicates these helpers.

## Plugins

MUST:

- Implement `ISRTBasicConfigurations` with a public parameterless constructor.
- Use `SRTFeatureNames` string constants.
- Keep plugin `Order` stable (Logs 10, SQL 20, PostgreSQL 21, Redis 30, Rabbit 40, Swagger 50).

MUST NOT:

- Skip copying the plugin assembly to the host output directory.

## Data / cache / errors

MUST:

- Share one scoped context across UoW repositories; never dispose it in a repo.
- Use `ICacheRepository` / `IErrorLogger` contracts from this package; implementations live in DB packages.

MUST NOT:

- Put EF types on Core interfaces.

## Documentation

MUST keep filenames prefixed with `SRT.Core.` when copying. Update `API_REFERENCE.md` when public contracts change. Keep `AGENTS` / `CLAUDE` / `CURSOR_RULE` as short pointers to this file.
