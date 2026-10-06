# SRT.Core — Codex / Agents entry

Package: **SRT.Core**

Before any change that uses or modifies this package, read:

1. [SRT.Core.AI_RULES.md](SRT.Core.AI_RULES.md) *(canonical MUST / MUST NOT)*
2. [SRT.Core.API_REFERENCE.md](SRT.Core.API_REFERENCE.md)
3. [SRT.Core.COOKBOOK.md](SRT.Core.COOKBOOK.md) when changing host startup

Working rule: read docs first; do not invent APIs; if ambiguous, inspect related code and update the docs.

Consumer API controllers inherit `SRTBaseControllerSystem` (see AI_RULES Controllers).

This file does not replace `AI_RULES.md`. Keep the `SRT.Core.` filename prefix when copying into consumer projects.
