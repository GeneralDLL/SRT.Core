# SRT.Core — Documentation Index

Package: **SRT.Core**  
Target: .NET 10  
Project: `src/SRT.Core/SRT.Core.csproj`  
Reviewed against: Core repo `95b0a5b`

All filenames in this folder start with `SRT.Core.` so you can copy the whole set into any consumer project without colliding with other packages’ docs.

This folder is **not** the GitHub homepage. The GitHub README is the unprefixed `README.md` at the repo root.

## Reading order

| Audience | Start here | Then |
|----------|------------|------|
| Humans | [SRT.Core.HUMAN_GUIDE.md](SRT.Core.HUMAN_GUIDE.md) | [COOKBOOK](SRT.Core.COOKBOOK.md), [API_REFERENCE](SRT.Core.API_REFERENCE.md) |
| AI (all tools) | [SRT.Core.AI_RULES.md](SRT.Core.AI_RULES.md) | [API_REFERENCE](SRT.Core.API_REFERENCE.md), [COOKBOOK](SRT.Core.COOKBOOK.md) |
| Codex | [SRT.Core.AGENTS.md](SRT.Core.AGENTS.md) | AI_RULES |
| Claude | [SRT.Core.CLAUDE.md](SRT.Core.CLAUDE.md) | AI_RULES |
| Cursor | Install [SRT.Core.CURSOR_RULE.mdc](SRT.Core.CURSOR_RULE.mdc) | AI_RULES |
| Copy paths per AI | [SRT.Core.AI_INSTALL.md](SRT.Core.AI_INSTALL.md) | — |

## Files in this set

1. `SRT.Core.README.md` — this index
2. `SRT.Core.HUMAN_GUIDE.md` — how the host pipeline works
3. `SRT.Core.API_REFERENCE.md` — exact contracts
4. `SRT.Core.COOKBOOK.md` — host recipes
5. `SRT.Core.AI_RULES.md` — canonical MUST / MUST NOT
6. `SRT.Core.AGENTS.md` — Codex entry
7. `SRT.Core.CLAUDE.md` — Claude entry
8. `SRT.Core.CURSOR_RULE.mdc` — Cursor rule **template**
9. `SRT.Core.AI_INSTALL.md` — which files to copy where

## Copy into a consumer project

Follow **[SRT.Core.AI_INSTALL.md](SRT.Core.AI_INSTALL.md)**. Files under package `docs/` are **not** auto-loaded.

## Session bootstrap (paste for Cursor / Claude / Codex)

```text
You are working with SRT.Core.
Before changing or consuming this package:
1) Read SRT.Core.AI_RULES.md
2) Use SRT.Core.API_REFERENCE.md for signatures/contracts
3) Follow SRT.Core.COOKBOOK.md for host Program / BasicProjectConfig / StartAppConfigurations
Rules: read docs first; do not invent APIs; if ambiguous, inspect related code and update these docs.
Keep Program.cs thin. Do not call UseAuthorization or MapControllers in the host.
```
