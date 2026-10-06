# SRT.Core — Claude entry

Package: **SRT.Core**

## Required reading

- [SRT.Core.AI_RULES.md](SRT.Core.AI_RULES.md) — follow entirely
- [SRT.Core.API_REFERENCE.md](SRT.Core.API_REFERENCE.md) — signatures and contracts
- [SRT.Core.COOKBOOK.md](SRT.Core.COOKBOOK.md) — host recipes

## Working rule

Read docs first. Do not invent APIs. If ambiguous, inspect related code and update these docs.

Keep `Program.cs` thin. Do not call `UseAuthorization` or `MapControllers` in the host. `SrtPluginLoader` is internal.

Consumer API controllers inherit `SRTBaseControllerSystem` (see AI_RULES Controllers).

Keep the `SRT.Core.` filename prefix when copying into consumer projects. This entry file is a pointer only; `AI_RULES.md` is authoritative.
