# MultiShell

MultiShell is a portable Windows terminal workspace evolved directly from MultiKilo. It keeps MultiKilo's native Windows Terminal stack and session isolation, then generalizes the launcher from one Kilo session per project to multiple CLI harnesses per folder.

The application is intentionally a thin workspace/session manager. Terminal input semantics belong to the native Windows Terminal control, not to WPF or managed code.

## Product model

The sidebar groups shells by folder:

```text
MyProject
  Kilo Code
  Claude Code
  OpenAI Codex

AnotherProject
  Gemini CLI
  PowerShell 7
```

Rules:

- A folder may run several different CLI presets at the same time.
- A folder may have at most one shell for a given preset.
- Switching items only changes which already-running terminal control is visible.
- `Open with another shell...` and the folder `+` button add another CLI to the same folder group.
- There is one visible terminal viewport; MultiShell does not provide grid or split panes.

Each shell exposes:

- **Start / Resume**: preset-specific behavior. AI CLIs use their native resume mechanism when supported; ordinary system shells simply start.
- **Restart**: Stop, then Start / Resume.
- **Stop**: terminates only that shell's process tree.
- **Standard / Full Approval**: stored per shell. Full Approval is exposed only when the CLI has a session-local launch option; MultiShell never edits a CLI's global permission configuration to simulate it.

Changing approval mode affects the next Start/Restart. It does not hot-switch an already-running process.

## Included presets

AI coding harnesses:

- Kilo Code
- Claude Code
- OpenAI Codex
- Gemini CLI
- OpenCode
- GitHub Copilot CLI
- Cursor CLI
- Amp
- Aider

System/custom:

- Command Prompt
- Windows PowerShell
- PowerShell 7
- WSL
- Custom command

Presets are launch knowledge, not bundled CLI installations. MultiShell detects executables on the portable app directory and `PATH`, allows an explicit executable path, and remembers the user's choice. Missing presets remain visible as **Not available**.

MultiShell does not install or update third-party CLIs.

## Portable app

MultiShell is published as a self-contained x64 folder. No MultiShell installer and no PowerShell 7 dependency are required.

Normal executable presets launch directly:

```text
MultiShell (WPF)
  -> native Windows Terminal control
  -> TerminalConnection / OpenConsole / ConPTY
  -> selected CLI
```

A shell intermediary is used only when required by the selected target, for example a `.cmd`, `.bat`, `.ps1` shim, or a Custom command explicitly configured to run with a shell.

Persistent portable state lives beside the executable:

- `shells.json` - shell definitions.
- `preferences.json` - remembered Add Shell choices and UI preferences.

Runtime process/terminal state is never persisted.

## MultiKilo terminal invariants

MultiShell carries forward MultiKilo's patched, pinned Windows Terminal runtime. The matched native runtime set must stay together:

- `Microsoft.Terminal.Control.dll`
- `TerminalConnection.dll`
- `OpenConsole.exe`
- `OpenConsoleProxy.dll`

The native terminal layer owns:

- keyboard input and Vietnamese IME/composition;
- mouse input, selection and scrolling;
- copy/paste shortcuts and clipboard filtering;
- bracketed paste and terminal mode state;
- VT parsing and rendering;
- ConPTY transport, resize and I/O ordering.

MultiShell/WPF owns only folder/shell orchestration, preset launch behavior, visibility, lifecycle, persistence, tray behavior and updates.

### Paste is not a managed feature

The correct large-paste path remains:

```text
Windows clipboard
  -> native TerminalControl paste
  -> Windows Terminal filtering / bracketed paste
  -> ConPTY
  -> CLI
```

Do not reintroduce managed fake typing, delayed/chunked paste queues, WPF Ctrl+V interception, synthetic bracket markers, a managed VT parser, or a custom managed ConPTY transport.

The 288-line paste regression that drove MultiKilo's native-terminal work is still a release-critical check for MultiShell.

## Session isolation

Each native terminal connection retains MultiKilo's Job Object ownership. Stopping one shell kills only that shell's process tree. Do not replace this with process-name killing.

Different CLI presets in the same folder may run concurrently. MultiShell deliberately does not try to coordinate file edits between agents; that remains the user's repository/Git workflow.

## Add Shell behavior

Add Shell remembers the last selected preset, approval mode, custom-command settings, folder and preferred executable.

- One detected executable: use it automatically.
- Multiple detected executables: allow selection and remember it.
- Remembered executable missing: fall back to current auto-detection.
- Preset not found: show **Not available** and allow Browse to an explicit executable.
- A preset already present in the selected folder cannot be added again.

Full Approval asks for a one-time acknowledgement per CLI preset.

## Tray and single-instance behavior

MultiShell is a single-instance application.

- Closing while shells are running hides the app to the system tray.
- Minimizing hides to tray.
- The tray notice is shown at most once per Windows boot.
- Launching MultiShell while an instance already exists restores the existing window instead of starting a second workspace.
- Tray **Quit** confirms before stopping live shells.

## Self-update

MultiShell can check its GitHub releases for a newer portable build.

- Updates are optional.
- `Update & Restart` downloads `MultiShell-win-x64.zip`, stops running shells after confirmation, replaces application/runtime files in the same portable folder and restarts.
- `shells.json` and `preferences.json` are preserved.
- Third-party CLI versions are never changed.

Tagged releases (`v*`) build, run the full native smoke suite and publish the expected portable ZIP.

## Requirements

- Windows 10 1809+ or Windows 11, x64.
- At least one desired CLI/shell available on `PATH` or selected explicitly.
- Individual AI CLI authentication remains the responsibility of that CLI.

## Build

From the repository root:

```powershell
.\build.ps1
```

Portable output:

```text
dist\MultiShell-win-x64\
```

The native terminal artifacts are built from the pinned Windows Terminal source and copied into the output as a matched set.

## Verification

CI contains four smoke areas:

- `presets` - required preset catalog, Start/Resume/Full mappings and direct-vs-shim launcher behavior.
- `clipboard` - native copy/selection/scroll.
- `paste` - Windows Terminal identity, 288-line bracketed paste and Vietnamese/Unicode paste.
- `sessions` - multiple native sessions, resize, switching and Job Object isolation.
- `all` - all of the above.

Run one mode:

```powershell
.\.github\scripts\Run-Smoke.ps1 presets
.\.github\scripts\Run-Smoke.ps1 paste
```

A release/full-acceptance run also publishes and inspects the portable folder.

## Non-goals

MultiShell is deliberately not:

- a new terminal emulator;
- Electron/Tauri/WebView2/xterm.js;
- a grid/split-pane terminal manager;
- an IDE or file explorer;
- an AI-agent orchestrator;
- a CLI installer/package manager;
- a plugin framework;
- a multi-session manager for the same CLI in one folder.

If a terminal behavior is already owned by Windows Terminal or the selected CLI, keep it there.
