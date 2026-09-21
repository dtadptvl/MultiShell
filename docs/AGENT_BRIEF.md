# Agent Brief

## Goal

Evolve MultiKilo into **MultiShell**, a portable Windows workspace for running multiple CLI harnesses while preserving MultiKilo's existing native Windows Terminal behavior. MultiShell should remain a thin session/workspace wrapper, not a new terminal emulator.

## Requirements

- Group sidebar items by folder. A folder may run different CLI presets concurrently, but may have only one shell for each preset.
- Include presets for Kilo Code, Claude Code, OpenAI Codex, Gemini CLI, OpenCode, GitHub Copilot CLI, Cursor CLI, Amp, Aider, cmd, Windows PowerShell, PowerShell 7, WSL, plus Custom command.
- Shell actions are **Start/Resume**, **Restart**, and **Stop**. Restart means Stop + Start/Resume. Each preset may define its own native resume behavior.
- Each shell stores **Standard** or **Full Approval**. Full Approval must use a session-local/native CLI option only; if that is not available, do not emulate it through global CLI configuration.
- Add Shell remembers the user's last choices. Auto-detect executables, allow an explicit path, remember that path, and fall back to detection if it disappears. Keep unavailable presets visible.
- MultiShell itself is a portable folder containing its executable/dependencies. Do not require PowerShell 7 or route every CLI through `pwsh`. Do not install/update external CLIs.
- Keep MultiKilo-style tray behavior and enforce one MultiShell app instance. A second launch restores the existing window. Show the minimize-to-tray notice only once per Windows boot.
- Support optional portable self-update without losing shell definitions/preferences.

## Constraints

Preserve MultiKilo's native Windows Terminal/ConPTY path, Job Object session isolation, clipboard, bracketed large paste, Vietnamese IME/input, mouse, selection and scroll behavior. Do not reimplement terminal input/paste semantics in WPF/managed code.

No grid or split panes.

## Out of scope

Multiple sessions of the same CLI in one folder, CLI installation/update, IDE features, agent orchestration, and a plugin framework.

## Implementation

Inspect the existing MultiKilo-derived codebase and follow its proven terminal patterns. Make the smallest reasonable changes outside the launcher/workspace layer. Avoid unrelated refactors and run the relevant Windows/native smoke checks.
