# AGENTS.md

This project uses **Serena** (LSP/AST-aware MCP server) as its knowledge and
code-navigation backend. Before doing anything non-trivial:

1. Call the `serena_initial_instructions` tool (or `mcp` with
   `instructions: "serena"`) and read the Serena Instructions Manual.
2. Read project memory `mem:core` (via `serena_read_memory`), then follow its
   references into the memory graph:
   - `mem:architecture` - startup flow, core singletons, power/auto-mode engine, UI structure, threading
   - `mem:hardware` - the 5 hardware I/O layers (ATKACPI, AsusHid USB, Pawn, GPU vendor APIs, WMI/Win32)
   - `mem:modules` - directory-by-directory module guide
   - `mem:configuration` - config.json format, per-mode/zone key naming, model detection (`Is*()`), i18n
   - `mem:peripherals` - mouse/keyboard/headset framework, adding a new device
   - `mem:tech_stack` - build, versioning, release pipeline, CI
   - `mem:conventions` and `mem:task_completion` - code rules and definition of done
3. Prefer Serena symbol tools (`find_symbol`, `find_referencing_symbols`,
   `get_symbols_overview`, `replace_symbol_body`, `get_diagnostics_for_file`)
   over raw grep for C# navigation and edits.

## Quick reference

- Project: G-Helper, C# WinForms, `net10.0-windows`, x64 only. Single project: `app/GHelper.sln`.
- Build: `dotnet build app/GHelper.sln` (on Linux add `-p:EnableWindowsTargeting=true`; dotnet is in `~/.dotnet`).
- The app only runs on Windows with ASUS hardware; hardware behavior needs a real device. Log: `%AppData%\GHelper\log.txt`.
- Hard rules (i18n via `Strings.resx`, model gating via `AppConfig.Is*()`, hardware I/O only through the established layers, nullable best-effort hardware reads with `Logger.WriteLine`, RForm theming, debounced hardware re-apply) are in `mem:conventions`.
