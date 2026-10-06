# SRT.Core

Shared host pipeline, `AppSetting`, connection strings, plugin contract, and cache/error/data interfaces for SRT microservices.

**Target:** .NET 10 (`net10.0`)

This GitHub README is the public homepage. The AI/human **package docs set** (nine prefixed files) is separate:

**[src/SRT.Core/docs/SRT.Core.README.md](src/SRT.Core/docs/SRT.Core.README.md)**

## Consume

Project reference (packages are not published from these repos as a documented NuGet feed):

```xml
<ProjectReference Include="..\SRT.Core\src\SRT.Core\SRT.Core.csproj" />
```

## Quick start

Keep the web host `Program.cs` thin:

```csharp
var dmAppSetting = args.SRT_ConfigController();
var (builder, config) = StartAppConfigurations.SRT_ConfigController(args, dmAppSetting);
var app = await builder.SRT_ConfigBuilder(config)
    .SRT_ConfigBuilderAndBuildApp(config)
    .SRT_ConfigApp(config);
app.Run();
```

Do not call `UseAuthorization` or `MapControllers` in the host. Project-reference every plugin `AppSetting.GetRequestedFeatures()` will request (Logs, SQL Server, PostgreSQL, Redis, RabbitMQ, Swagger).

## Related repositories

| Repo | Role |
|------|------|
| SRT.Core.Logs | Serilog plugin (`SRT.Core.SRTLogs`) |
| SRT.Core.Database.SQLServer | EF SQL Server |
| SRT.Core.Database.Postgresql | EF PostgreSQL |
| SRT.Core.Database.Redis | Cache |
| SRT.Core.MessageBroker.RabbitMQ | Messaging |
| SRT.Core.Swaggers | Swagger plugin |
| SRT.Core.Files | File helpers (`SRT.Core.SRTFiles`) |
| SRT.Core.TestApplications | Sample hosts |
