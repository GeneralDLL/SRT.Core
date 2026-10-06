# SRT.Core — Cookbook

Recipes for consuming this package. Signatures match [SRT.Core.API_REFERENCE.md](SRT.Core.API_REFERENCE.md).

---

## 1. Host startup (self-contained)

### `Program.cs`

```csharp
var dmAppSetting = args.SRT_ConfigController();
var (builder, config) = StartAppConfigurations.SRT_ConfigController(args, dmAppSetting);
var app = await builder.SRT_ConfigBuilder(config)
    .SRT_ConfigBuilderAndBuildApp(config)
    .SRT_ConfigApp(config);
app.Run();
```

Do **not** call `UseAuthorization` / `MapControllers` here.

### `BasicProjectConfig` (web project)

```csharp
public static class BasicProjectConfig
{
    public static MyAppSetting SRT_ConfigController(this string[] args)
    {
        return new MyAppSetting
        {
            UseSerilog = true,
            UseErrorLogSQL = true,
            UseCashStructure = CashType.None, // or CashNoSQL / CashWithSQL
        };
    }

    public static WebApplicationBuilder SRT_ConfigBuilder(this WebApplicationBuilder builder, MyAppSetting config)
    {
        // Local DI only
        return builder;
    }

    public static async Task<WebApplication> SRT_ConfigApp(this Task<WebApplication> appTask, MyAppSetting config)
    {
        var app = await appTask;
        return app.SRT_ConfigApp(config);
    }

    public static WebApplication SRT_ConfigApp(this WebApplication app, MyAppSetting config)
    {
        // Local middleware only
        return app;
    }
}
```

### `StartAppConfigurations` (GeneralDLL)

Must call Core:

```csharp
public static (WebApplicationBuilder builder, MyAppSetting config)
    SRT_ConfigController(string[] args, MyAppSetting config)
{
    return BaseConfigurations.SRT_ConfigController(args, config, AddAuthentication);
}
```

`AddAuthentication` is `Func<IServiceCollection, URLData, IServiceCollection>`.

### Consume

```xml
<ProjectReference Include="..\path\SRT.Core\src\SRT.Core\SRT.Core.csproj" />
```

If `UseSerilog` is true, also reference `SRT.Core.Logs` (`SRT.Core.SRTLogs.csproj`). Same pattern for every requested feature.

---

## 2. Connection strings

`appsettings.json` section `ConnectionDB` binds to `AppConnectionString`. Example Redis plaintext:

```json
"ConnectionDB": {
  "SRTCore_CacheRedis": {
    "connectionType": "redis",
    "host": "127.0.0.1",
    "port": "6379",
    "sslEnabled": "false"
  }
}
```

TLS: `"sslEnabled": "true"` and/or `"caPath": "tls/ca.pem"`. Missing CA file fails fast.

Also bind:

```json
"Service": { "Id": 1, "Name": "MyService" }
```

---

## 3. `MyAppSetting`

```csharp
public class MyAppSetting : AppSetting, IHasSwaggerConfig
{
    public SwaggerDomain? SwaggerConfig { get; set; }
}
```

Implement `IHasSwaggerConfig` only if you ship Swagger. A non-null `SwaggerConfig` requests the Swagger plugin.

---

## 4. New application layout

`src/{Name}/{Name}` (GeneralDLL) and `src/{Name}/{Name}.Web`. Do not reuse a shared `SRT.Test` GeneralDLL for a new app.

---

## 5. API controller (self-contained)

Every JSON API controller in a consumer host inherits `SRTBaseControllerSystem`. Put `[Route]` on the derived type. Local services stay constructor-injected.

```csharp
using Microsoft.AspNetCore.Mvc;
using SRT.Core.SYS_Controllers;

namespace MyService.Controllers;

[Route("items")]
public sealed class ItemsController : SRTBaseControllerSystem
{
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Response_Error_BadRequest("id is required");

        return Response_OK(data: new { id });
    }
}
```

Do not return `Ok(item)` or `NotFound()`. Read the user with `GetUserInfo()` when auth middleware ran. Use `Response_Error_ServiceUnavailable` for a down dependency and `Response_Error_GatewayTimeout` for an upstream wait timeout. Files/streams and `Response_NoContent()` are the envelope exceptions.
