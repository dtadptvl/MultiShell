# MultiShell 2.0 — Agent Brief (Stages 0–3)

**Purpose:** Implementation guardrails for coding agents working on the MultiShell 2.0 foundation.  
**Canonical product/architecture source:** `docs/ARCHITECTURE_2_0.md`.  
**Current legacy brief:** `docs/AGENT_BRIEF.md` remains useful for MultiShell 1.x behavior, but this file takes precedence for 2.0 Stage 0–3 work.

---

## Goal

Evolve the current MultiKilo-derived Windows app into the MultiShell 2.0 Host architecture **without regressing the native Windows Terminal behavior that already works**.

The immediate target is not Cloud, WebRTC, mobile, Team, or SaaS.

The immediate target is:

```text
Stage 0  polish current Windows product
Stage 1  extract Core/Protocol boundaries
Stage 2  introduce Host + local IPC/state ownership
Stage 3  move PTY ownership to Host safely
```

After Stage 3, killing the WPF UI must no longer kill running shells.

---

## Non-Negotiable Invariants

1. **Host is the future source of truth.**
2. **Connection/UI never owns a shell.**
3. **Protocol/business semantics must remain transport-independent.**
4. **Windows native terminal fidelity must not regress.**
5. **Never reintroduce managed fake typing.**
6. **Never reintroduce delayed/chunked text paste.**
7. **Never intercept normal terminal paste in WPF to synthesize bracket markers.**
8. **Never replace the native Windows Terminal VT/input stack with a managed parser/terminal emulator.**
9. **Do not force all CLIs through PowerShell.**
10. **If a logical command works in a normal terminal, MultiShell should be able to launch it without asking the user for `.cmd`, `.ps1`, or a path.**
11. **UI crash must not stop shells after Stage 3B.**
12. **Host crash may stop shells in MultiShell 2.0. Do not implement persistent PTY-through-Host-crash yet.**
13. **Ordinary updates must never silently stop live shells.**
14. **Local functionality must remain independent of Cloud/subscription.**
15. **Do not implement IDE, file-manager, VPN, remote-desktop, agent-orchestration, or plugin-marketplace features while working on Stages 0–3.**

---

## Current Proven Windows Terminal Stack

The current app intentionally uses:

```text
WPF
→ TerminalWpfPatched
→ native Windows Terminal HwndTerminal
→ Windows Terminal ConptyConnection
→ ConPTY/OpenConsole
→ CLI process
```

The pinned Windows Terminal patch exists specifically to preserve:

- Windows Terminal native renderer/input;
- TSF/IME behavior;
- native key processing;
- native clipboard/copy;
- native large/bracketed paste;
- mouse/selection/scroll;
- Job Object process isolation.

Do **not** replace this stack casually.

Relevant files include:

```text
src/MultiShell/Terminal/ShellSession.cs
src/MultiShell/Terminal/TerminalSmokeTest.cs
src/TerminalWpfPatched/TerminalControl.xaml.cs
src/TerminalWpfPatched/TerminalContainer.cs
src/TerminalWpfPatched/NativeMethods.cs
eng/windows-terminal/Apply-Patch.ps1
eng/windows-terminal/README.md
docs/TESTING.md
```

---

## Important Existing Native Bridge

The pinned Windows Terminal patch already exposes a small C ABI around the real Windows Terminal `ConptyConnection`:

```text
MultiShellConptyCreate
MultiShellConptyWrite
MultiShellConptyResize
MultiShellConptyTerminate
MultiShellConptyIsRunning
MultiShellConptyDestroy
```

The terminal renderer/input side also exposes useful primitives such as:

```text
TerminalRegisterWriteCallback
TerminalSendOutput
TerminalSendKeyEvent
TerminalSendCharEvent
native copy/paste paths
```

**Use these seams before inventing a new ConPTY backend.**

The intended Stage 3 architecture is to split:

```text
native renderer/input
        ↕
external PTY runtime
```

while keeping both halves based on the same proven Windows Terminal components.

---

# Stage 0 — MultiShell 1.x Polish

## Objectives

- preserve current behavior;
- make CLI launching lean;
- remove path/extension trivia from normal UX;
- improve UI without changing terminal semantics.

## CLI Resolution Rule

User-facing preset:

```text
Kilo
command: kilo
```

not:

```text
kilo.cmd
kilo.ps1
C:\...\kilo.cmd
```

Windows deterministic resolution preference:

```text
.exe
.com
.cmd
.bat
.ps1
```

Rules:

- `.exe/.com` direct;
- `.cmd/.bat` through `cmd.exe /d /s /c`;
- `.ps1` through Windows PowerShell only when actually required;
- manual executable selection is an Advanced escape hatch;
- never route all CLIs through `pwsh`.

## Kilo Mapping Is Authoritative

```text
Standard fresh:
kilo

Standard resume:
kilo --continue

Full fresh:
kilo --auto

Full resume:
kilo --auto --continue
```

Do not change this mapping without explicit product approval.

## Stage 0 Gate

All existing terminal regression tests and manual checks remain green.

No terminal architecture refactor is allowed merely to finish Stage 0 polish.

---

# Stage 1 — Core Extraction

## Goal

Remove business rules from WPF without changing runtime behavior.

Create or move toward:

```text
src/MultiShell.Core/
src/MultiShell.Protocol/
```

`MultiShell.Core` must not depend on:

- WPF;
- WinForms;
- `TerminalControl`;
- HWND;
- Windows Terminal native DLLs.

## Core Concepts

Core owns concepts such as:

```text
ShellDefinition
ApprovalMode
CliPreset
CliPresetCatalog
ShellRuntimeState
ShellSnapshot
Shell launch intent
folder+preset uniqueness rule
lifecycle semantics
```

Keep these distinctions:

```text
Runtime:
Stopped
Starting
Running
Stopping

Availability:
Available
CLI unavailable

Attention:
separate concern
```

Do not turn `CLI unavailable` or `Needs Attention` into process states.

## ShellSession Warning

The current `ShellSession` mixes:

```text
business lifecycle
+
WPF TerminalControl
+
ConPTY/process ownership
```

Do not move that class wholesale into Core.

Refactor around responsibility boundaries.

## Lifecycle Service

Business operations should converge on a service surface such as:

```text
CreateShell
StartShell
StopShell
RestartShell
RemoveShell
ChangeApproval
```

Rules must live in the service/Core, not only in dialogs.

For example:

> same normalized folder + same preset = one shell maximum.

This must remain true even when future Web/mobile clients call the service.

## Lifecycle Serialization

Operations on the same `shellId` must be serialized.

Desired behavior:

```text
Start while Running → success/no-op
Stop while Stopped → success/no-op
double Stop → safe
Restart → serialized Stop + Start/Resume
```

Design for future multiple clients now, even though Stage 1 is still local.

## Stage 1 Gate

- existing user behavior unchanged;
- current terminal path unchanged;
- existing terminal smoke tests green;
- new Core tests cover uniqueness, lifecycle, approval, preset mappings, and launch intent.

---

# Stage 2 — Host / Protocol Skeleton

## Goal

Introduce the future source-of-truth process and local protocol **without moving PTY ownership yet unless Stage 3 prototype has proven safe**.

Create:

```text
src/MultiShell.Host/
src/MultiShell.Protocol/
```

## Target Responsibility Split

Host begins owning:

- shell definitions;
- `shells.json`;
- authoritative shell metadata;
- lifecycle intent/state model.

WPF owns:

- rendering;
- dialogs;
- selection;
- client preferences.

During the Stage 2 transition, WPF may temporarily still own the live PTY.

That is transitional only. Do not preserve dual runtime ownership long term.

## Local IPC

Windows:

```text
Named Pipe
```

Future macOS/Linux:

```text
Unix Domain Socket
```

Local protocol semantics must match the future Remote semantics.

Do not put business rules into the transport layer.

## Protocol Basics

Stage 2 needs at least:

```text
Handshake
GetSnapshot

CreateShell
RemoveShell
StartShell
StopShell
RestartShell
ChangeApproval

state-change events
```

Handshake should allow future version/capability negotiation:

```text
AppVersion
ProtocolMin
ProtocolMax
Capabilities
```

Mutating requests should carry a `requestId` so retries can be deduplicated later.

State/event flow should support a monotonic revision and `GetSnapshot` recovery.

## Persistence Rule

After Stage 2, Host is the only writer of `shells.json`.

Do not let WPF and Host both write it.

Keep `shells.json` simple and additive through MultiShell 2.x.

Client-specific preferences stay client-side:

```text
theme
window position
sidebar size
last selected shell
```

## Local IPC Security

Named Pipe should be restricted to the current Windows user.

Do not add Cloud-style JWT/OAuth complexity to local IPC.

## Stage 2 Gate

- WPF no longer directly owns shell persistence;
- Host starts/connects reliably;
- second UI instance does not create a second Host;
- snapshots/events rebuild the sidebar correctly;
- terminal behavior remains unchanged;
- all current terminal regressions remain green.

---

# Stage 3A — External PTY + Native Renderer, Same Process

## This Is the Critical Prototype Gate

Do **not** move PTY across process boundaries first.

First prove that the native renderer/input can operate against an external PTY runtime while both still live in the same process.

Target:

```text
External ConPTY runtime
        │
 output │ │ input
        ▼ ▲
native TerminalControl
```

Do not involve Named Pipe, WebRTC, Cloud, or Protobuf hot-path framing yet.

This isolates terminal-decoupling risk from IPC risk.

## Required Renderer Surface

The terminal component should conceptually support a narrow external-session mode:

```text
AttachExternalSession
FeedOutput
InputProduced
ResizeRequested
```

Naming may differ, but the responsibility boundary must stay narrow.

`TerminalWpfPatched` must not learn about:

- shellId;
- Named Pipe;
- Protobuf;
- WebRTC;
- Cloudflare;
- Account;
- Space;
- Remote.

## Keyboard / IME Path

Must remain native:

```text
Windows keyboard / IME
        ↓
native HwndTerminal
        ↓
Windows Terminal input logic
        ↓
TerminalRegisterWriteCallback
        ↓
external PTY runtime
```

Forbidden:

```text
WPF KeyDown
→ custom key translation
→ synthetic terminal bytes
```

## Paste Path

Must remain native:

```text
Ctrl+Shift+V / Shift+Insert
        ↓
native terminal clipboard path
        ↓
FilterStringForPaste
        ↓
TerminalCore bracketed-paste state
        ↓
native write callback
        ↓
external PTY runtime
```

Forbidden:

- managed fake typing;
- line-by-line/chunk-delay paste;
- synthetic `ESC[200~` / `ESC[201~` from WPF;
- copying clipboard text into a managed loop.

If external-session mode requires those regressions, **the prototype fails**. Do not continue that design.

## Output Path

Use:

```text
PTY output
→ TerminalSendOutput
→ native TerminalCore
→ native renderer
```

Do not add a managed VT parser.

## Resize Path

Native renderer computes rows/columns.

External runtime receives:

```text
Resize(columns, rows)
```

The Host/runtime does not need to know cell metrics or WPF pixels.

## PTY Runtime

Prefer the already-patched real Windows Terminal connection bridge:

```text
MultiShellConptyCreate
MultiShellConptyWrite
MultiShellConptyResize
MultiShellConptyTerminate
MultiShellConptyIsRunning
MultiShellConptyDestroy
```

Do not write a parallel ConPTY implementation unless the existing bridge is proven inadequate.

## Stage 3A Gate

Run the complete existing smoke suite plus manual verification.

Required checks:

- keyboard input;
- Vietnamese IME composition;
- Unicode Vietnamese text;
- native copy;
- native paste;
- 288-line bracketed paste smoke;
- 20 / 200 / 1000 line paste fixtures;
- 1 MB mixed Unicode/Vietnamese paste;
- image paste behavior currently supported by the product;
- selection;
- scroll;
- mouse;
- resize;
- ANSI color;
- full-screen TUI;
- multiple simultaneous shell sessions;
- terminating one session does not affect others;
- project switching does not restart background sessions.

**Any meaningful native regression means Stage 3A is not complete.**

---

# Stage 3B — Move PTY Runtime Into MultiShell.Host

Only begin this after Stage 3A is green.

Target:

```text
MultiShell.exe / WPF
        │
     Named Pipe
        │
MultiShell.Host.exe
        │
WindowsTerminalConptyRuntime
        │
ConPTY / CLI
```

Do not change native terminal semantics while moving the already-proven external runtime across IPC.

## Runtime Abstraction

Introduce/retain a small runtime boundary similar to:

```text
IShellRuntimeSession

Start
WriteInput
Resize
Stop

Output
Exited
IsRunning
```

Windows implementation:

```text
WindowsTerminalConptyRuntime
```

Future:

```text
PosixPtyRuntime
DaemonBackedRuntime
```

Do not make WPF types leak into the Host runtime.

## Terminal Stream

Control/state messages may use Protobuf.

Terminal hot path should remain compact:

```text
small binary frame
+
raw UTF-8 VT payload
```

Windows adapter may need incremental UTF-16 ↔ UTF-8 conversion.

Do not split Unicode characters incorrectly across chunks; add explicit Unicode/Vietnamese tests.

## Host Runtime State

A running shell may track:

```text
shellId
runtime state
PTY/native runtime handle
rows / columns
output sequence
bounded replay buffer
last exit code
```

It must not contain:

- WPF controls;
- HWND renderer ownership;
- Dispatcher dependencies.

## Attach / Detach Semantics

Selecting another shell in the UI detaches the view, not the process.

```text
AttachTerminal(shellId)
DetachTerminal(shellId)
```

Connection/view lifetime must not define shell lifetime.

## UI Crash Acceptance Test

Start multiple live shells, then hard-kill `MultiShell.exe`.

Expected:

```text
MultiShell.Host.exe alive
Kilo alive
Claude alive
Codex alive
```

Restart the UI.

Expected:

```text
connect Host
→ GetSnapshot
→ running states restored
→ selected terminal can reattach
```

The shells were never "resumed" because they never stopped.

## Replay

Host keeps a bounded in-memory raw-output buffer.

On reattach:

```text
feed available replay
→ native renderer rebuilds recent state
```

Do not implement a server-side VT screen parser merely for perfect scrollback reconstruction.

If old output has fallen out of the bounded buffer, recent context only is acceptable.

## Host Crash

Host owns the runtime/Job Object.

If Host crashes:

```text
managed shell trees may stop
```

This is acceptable in MultiShell 2.0.

Do not attempt process adoption or persistent PTY daemon work during Stage 3.

---

# Regression Gates

The following gates are mandatory throughout Stages 0–3.

## Automated Terminal Smoke

Run the repository's native terminal smoke checks, including:

```text
--smoke-test presets
--smoke-test clipboard
--smoke-test paste
--smoke-test sessions
--smoke-test all
```

Use the current project scripts/workflow as the authoritative invocation.

## Interactive Windows Verification

Follow `docs/TESTING.md`.

At minimum verify:

### Session lifecycle

1. Start at least three projects/folders.
2. Confirm independent live CLI sessions.
3. Switch repeatedly between them.
4. Background sessions continue running/outputting.
5. Run ANSI output and a full-screen TUI.
6. Resize/maximize/restore while TUI is active.
7. Stop one shell; the others remain alive.
8. Restart Kilo and confirm the approved resume mapping is used.

### Vietnamese input

Using a real Vietnamese IME:

1. Type Vietnamese directly into the CLI.
2. Test tone marks and multi-keystroke composition.
3. Edit in the middle of composed text.
4. Use arrows, Home/End, Backspace/Delete, Enter.
5. Reject duplicate characters, stale composition text, or broken caret movement.

### Clipboard / paste

Use the repository paste fixtures and test:

- 20 lines;
- 200 lines;
- 1000 lines;
- 1 MB mixed Unicode/Vietnamese text.

Verify:

- paste is bulk/fast;
- UI stays responsive;
- no simulated typing;
- Unicode preserved;
- bracketed paste preserved;
- no truncation;
- native copy still works.

### Image paste

If image paste is supported by the current release, verify it still follows the intended native/product path. A terminal refactor that silently loses image paste is a regression.

### Tray / UI lifetime

Before Stage 3B, preserve the current tray semantics.

After Stage 3B add the stronger invariant:

```text
hard-kill UI
→ Host and live shells survive
```

### Portable build

Run the published portable build outside the source tree and repeat at least:

- shell start;
- Vietnamese typing;
- 1 MB paste;
- resize;
- terminate;
- UI close/reopen/reattach as appropriate for the stage.

---

# Stop Conditions

An agent must stop and fix/revert the approach instead of pushing forward if any of these occur:

- native Vietnamese IME regresses;
- large paste becomes simulated typing, delayed, or visibly chunked;
- bracketed paste breaks;
- copy/selection/mouse/scroll regress;
- one shell operation kills unrelated shells;
- Stage 3 external runtime requires WPF key translation;
- Stage 3 external runtime requires managed VT parsing;
- WPF and Host both become writers of `shells.json`;
- shell lifetime accidentally becomes tied to a client/connection;
- a refactor begins expanding into IDE/VPN/remote-desktop/orchestration scope.

Preserve the working terminal path over architectural elegance.

---

# Completion Criteria for the Foundation

Stages 0–3 are complete only when all of the following are true:

1. Normal Add Shell no longer asks users to understand executable extensions/locations when the command is resolvable.
2. Business shell rules are outside WPF.
3. Host is the sole authoritative owner of shell definitions/persistence.
4. Native renderer/input and PTY runtime are decoupled without terminal-quality regression.
5. Host owns live PTY/process state.
6. Hard-killing/restarting the WPF UI does not terminate live shells.
7. Existing native terminal regression coverage remains green.
8. No Cloud/Remote architecture has contaminated the native terminal component.

Only then proceed to Stage 4 LAN Remote work.
