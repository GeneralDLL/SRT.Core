# SRT.Core — Where to copy files for each AI

Source folder (inside this package):

`SRT.Core/src/SRT.Core/docs/`

Copy **with the same filenames** (keep the `SRT.Core.` prefix).  
`<project>` = root of the consumer solution/repo that referenced this package.

Prefixed files under this `docs/` folder are **not** auto-loaded by Cursor, Claude, or Codex. Copying is required.

---

## Shared (all AIs) — human + full knowledge

| Copy from `docs/` | Paste into consumer |
|-------------------|---------------------|
| `SRT.Core.README.md` | `<project>/docs/` |
| `SRT.Core.HUMAN_GUIDE.md` | `<project>/docs/` |
| `SRT.Core.API_REFERENCE.md` | `<project>/docs/` |
| `SRT.Core.COOKBOOK.md` | `<project>/docs/` |
| `SRT.Core.AI_RULES.md` | `<project>/docs/` |

Optional: also keep `SRT.Core.AI_INSTALL.md` in `<project>/docs/`.

---

## Cursor

| Copy from `docs/` | Paste into consumer |
|-------------------|---------------------|
| `SRT.Core.CURSOR_RULE.mdc` | `<project>/.cursor/rules/` |
| `SRT.Core.AI_RULES.md` | `<project>/docs/` |
| `SRT.Core.API_REFERENCE.md` | `<project>/docs/` |
| `SRT.Core.COOKBOOK.md` | `<project>/docs/` |

Minimum for Cursor to apply rules automatically: the `.mdc` under `.cursor/rules/`.  
The rule points at the files under `docs/` — those three must exist there too. Do not overwrite unrelated `.mdc` files.

---

## Claude (Claude Code / Claude projects)

| Copy from `docs/` | Paste into consumer |
|-------------------|---------------------|
| `SRT.Core.CLAUDE.md` | `<project>/` (repo root) **or** `<project>/docs/` |
| `SRT.Core.AI_RULES.md` | `<project>/docs/` |
| `SRT.Core.API_REFERENCE.md` | `<project>/docs/` |
| `SRT.Core.COOKBOOK.md` | `<project>/docs/` |

Claude often auto-reads a root `CLAUDE.md`. Keep the package prefix. If the repo already has `CLAUDE.md`, add one line: `Also follow docs/SRT.Core.AI_RULES.md`. Do not rename this file to plain `CLAUDE.md` when other packages coexist.

---

## Codex (OpenAI Codex / AGENTS.md)

| Copy from `docs/` | Paste into consumer |
|-------------------|---------------------|
| `SRT.Core.AGENTS.md` | `<project>/` (repo root) **or** `<project>/docs/` |
| `SRT.Core.AI_RULES.md` | `<project>/docs/` |
| `SRT.Core.API_REFERENCE.md` | `<project>/docs/` |
| `SRT.Core.COOKBOOK.md` | `<project>/docs/` |

If the repo already has `AGENTS.md`, add a pointer: `Follow docs/SRT.Core.AI_RULES.md for SRT.Core.` Do not overwrite another package’s `AGENTS.md`.

Canonical rules for every AI: `docs/SRT.Core.AI_RULES.md`
