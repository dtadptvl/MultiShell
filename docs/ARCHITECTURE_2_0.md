# MultiShell 2.0 — Final Product & Architecture Brief

**Status:** Approved baseline  
**Purpose:** Source of truth for product, UX, pricing, account model, security, cloud, protocol, Host architecture, and implementation roadmap.  
**Lineage:** MultiKilo → MultiShell 1.x → MultiShell 2.0

---

## 1. Product Vision

MultiShell 2.0 is a **local-first, cross-platform shell workspace** for running and managing coding shells on the user's real machines, then continuing work with those same shells from another desktop, the web, iPhone, iPad, or Android.

Core proposition:

> **Your shells, anywhere.**

Commercial proposition:

> **Leave your desk without leaving your shells.**

MultiShell is not positioned as a generic remote-terminal product.

The user should not think:

```text
Connect to computer
→ find terminal
→ find folder
→ find process
```

The user should think:

```text
My work

● Kilo
  MultiShell · Home PC

● Claude !
  Website · MacBook

● Codex
  Backend · Linux
```

**Work/shell is the primary object. Device is context.**

---

## 2. What MultiShell Is

MultiShell manages this hierarchy:

```text
Space
→ Device
→ Folder
→ Shell
→ Terminal
```

Definitions:

- **Account** = human identity.
- **Space** = trust domain and subscription boundary.
- **Device** = cryptographic device identity.
- **Host** = a Device with the `ShellHost` capability that is Remote-enabled inside a Space.
- **Folder** = native working directory.
- **Shell** = Kilo / Claude / Codex / Gemini / PowerShell / Custom / other supported CLI.
- **Terminal** = presentation and input surface for a shell.

MultiShell is a **shell manager**, not:

- an IDE;
- a code editor;
- a full file explorer;
- a Git GUI;
- a remote-desktop suite;
- a VPN;
- a generic port-forwarding service;
- an agent-orchestration framework;
- a workflow builder;
- a plugin marketplace.

---

## 3. Core Philosophy

### 3.1 Local-first

Local MultiShell must be fully useful with:

```text
no account
no cloud
no subscription
```

Remote is an extension of the local workspace, not a separate product.

### 3.2 Shells run on the Host

The CLI runs directly on the user's Host:

```text
MultiShell Host
      ↓
Kilo / Claude / Codex / ...
      ↓
real OS user
real filesystem
real credentials
```

The Cloud does not:

- execute the CLI;
- store the repository;
- store SSH private keys;
- store API keys;
- store AI CLI credentials;
- store terminal transcripts by default.

### 3.3 Host OS permissions are authoritative

A trusted Remote user has authority equivalent to the user running the Host.

Remote may:

- choose any folder accessible to the Host OS user;
- create folders anywhere that user can;
- clone repositories anywhere that user can;
- launch a shell with any accessible cwd;
- use Standard / Full Approval;
- enter commands normally.

There is **no Project Roots security jail**.

MultiShell does not automatically:

- elevate UAC;
- run as SYSTEM/root;
- sudo;
- bypass filesystem permissions.

Security protects:

> **Who is allowed to access the Host?**

It does not attempt to protect a user from their own shell.

---

## 4. Competitive Positioning

MultiShell does not compete with 9Remote by feature count.

9Remote has already commoditized much of the generic remote-computer surface:

- remote terminal;
- mobile terminal;
- multi-host;
- persistent PTY;
- WebRTC;
- push;
- filesystem/Git workflows;
- site browser;
- remote desktop.

MultiShell must differentiate through the following.

### 4.1 Native local shell manager

MultiShell should be an app the user wants to use every day even when Remote is disabled.

### 4.2 CLI-aware lifecycle

MultiShell understands:

```text
Kilo
Claude
Codex
Gemini
...
```

not just `Terminal #4`.

### 4.3 Shell-first multi-host UX

Phone prioritizes:

```text
● Claude
  Website · MacBook
```

rather than:

```text
MacBook
→ Terminal #7
```

### 4.4 Continuity

```text
desktop
→ walk away
→ push
→ phone
→ same shell
→ respond
→ desktop
```

### 4.5 Needs Attention

Remote does not merely allow a connection. It helps answer:

> Which shell needs me?

and opens that exact shell.

---

## 5. Native Windows Quality Is an Invariant

MultiKilo/MultiShell has already solved difficult Windows terminal behavior:

- Vietnamese IME;
- native keyboard input;
- large paste;
- bracketed paste;
- image paste;
- copy and selection;
- mouse;
- scroll;
- resize;
- full-screen TUI;
- multi-session isolation.

MultiShell 2.0 must not regress these just to gain source-code uniformity.

Windows local remains:

```text
native Windows Terminal control
+
ConPTY
```

Cross-platform means **the same product behavior and model**, not necessarily the same implementation everywhere.

---

## 6. Folder / Shell Model

Folder ≈ working directory.

Examples:

```text
D:\Projects\MultiShell
/Users/me/src/foo
/home/me/work/bar
```

A folder may contain:

```text
Kilo
Claude
Codex
```

concurrently.

Invariant:

> **A folder may contain at most one shell of the same preset.**

Valid:

```text
Folder A
├ Kilo
├ Claude
└ Codex
```

Invalid:

```text
Folder A
├ Kilo
└ Kilo
```

---

## 7. Shell Lifecycle

Common actions:

```text
Start/Resume
Restart
Stop
```

Restart always means:

```text
Stop
+
Start/Resume
```

Resume semantics belong to each preset.

Authoritative Kilo mapping:

```text
Standard fresh
kilo

Standard resume
kilo --continue

Full fresh
kilo --auto

Full resume
kilo --auto --continue
```

Do not introduce a generic:

```text
New Session?
Resume Existing?
```

when a CLI has its own native resume semantics.

---

## 8. Approval Model

Each shell stores:

```text
Standard
Full
```

Full Approval:

- uses the CLI's native/session-local option;
- never edits global CLI configuration to emulate Full;
- is persisted per shell;
- applies on the next Start/Restart;
- does not hot-modify a currently running process.

UI may show:

```text
Kilo     FULL
```

The first Full enablement for a CLI gets a short warning.

If a CLI does not support it:

```text
Full Approval — Not supported
```

---

## 9. CLI Resolution

Principle:

> **If the command works when typed into a terminal, MultiShell should be able to launch it using that logical command.**

Presets use logical commands:

```text
kilo
claude
codex
```

Normal users should not need to know:

```text
kilo.cmd
kilo.ps1
C:\Users\...\kilo.cmd
```

Windows resolution preference:

```text
.exe
.com
.cmd
.bat
.ps1
```

Rules:

- `.exe/.com` direct;
- `.cmd/.bat` through `cmd.exe`;
- `.ps1` through PowerShell only when required;
- do not route every CLI through `pwsh`.

Executable path is visible only under:

```text
Advanced
Locate manually…
```

and is treated as an explicit override.

---

## 10. Product Plans

MultiShell has three levels:

```text
Free
Pro Solo
Pro Team
```

---

## 11. Free

**Price:** $0

Local-first. No account required.

Includes:

- unlimited local folders;
- unlimited local shells;
- all presets;
- Standard / Full Approval;
- Start/Resume/Restart/Stop;
- local notifications;
- local New Project;
- Windows/macOS/Linux local Host;
- local desktop UI;
- update;
- tray/background behavior.

Free must not be artificially crippled.

Remote Cloud is not included.

---

## 12. Pro Solo

**Pricing baseline:**

```text
$5/month
$48/year
```

20% annual discount.

Target:

> one individual user who wants remote access to one primary machine.

Includes:

- **1 active Remote Host**;
- unlimited client devices;
- desktop-client remote access;
- web;
- iPhone/iPad;
- Android;
- push notifications;
- Remote New Project;
- remote filesystem picker;
- P2P + relay fallback;
- unlimited ordinary remote use subject to fair use.

Critical distinction:

> **Solo limits Remote Hosts, not client devices.**

Example:

```text
Home PC
Host + Client
Remote Host slot ✅

Office PC
Client ✅
Remote Host ❌

MacBook
Client ✅
Remote Host ❌

iPhone
Client ✅

Chrome
Client ✅
```

Office PC may still use MultiShell Desktop as a client to control Home PC.

---

## 13. Pro Team

**Pricing baseline:**

```text
$20/month
$192/year
```

Flat subscription.

Includes:

- unlimited Remote Hosts;
- multiple member accounts;
- unlimited client devices;
- invite/remove members;
- shared MultiShell Space;
- all members can access Team Hosts;
- desktop/web/mobile remote;
- push;
- Remote New Project;
- P2P + relay;
- desktop-to-desktop remote;
- fair use.

There is no per-seat billing in v1.

There is no Host quota.

There is no client-device quota.

---

## 14. Space Is the Trust Domain

The old concept:

```text
Account = trust domain
```

is superseded.

Final model:

> **Account = human identity.  
> Space = trust domain.  
> Device = cryptographic identity.  
> Host = a Device providing ShellHost capability inside a Space.**

Solo:

```text
Alice Account
    ↓
Alice Personal Space
    ↓
Home PC Remote Host
```

Team:

```text
Alice ─┐
Bob ───┼→ Acme Space
Carol ─┘      │
              ├ Alice PC
              ├ Bob MacBook
              ├ Build Linux
              └ Mac mini
```

---

## 15. An Account Can Join Multiple Spaces

Example:

```text
Alice

Spaces
├ Personal
├ Acme Startup
└ Open Source Team
```

No duplicate user account is required.

A Space switcher may appear when relevant.

---

## 16. A Host Belongs to One Space

Invariant:

> **A Remote Host belongs to exactly one Space at a time.**

Do not allow:

```text
Home PC
├ Space A
├ Space B
└ Space C
```

simultaneously.

A user may move a Host between Spaces.

An Account may belong to multiple Spaces.

A Client may access multiple Spaces.

---

## 17. Desktop-to-Desktop Remote

Do not describe this as:

```text
Host controls Host
```

Correct model:

> **A trusted MultiShell desktop client can control another Remote Host, subject to Space entitlement.**

A desktop installation may expose:

```text
Desktop Device
├ DesktopClient
└ ShellHost
```

### Solo

Office PC can act only as a client:

```text
Office PC Desktop UI
        ↓
Home PC Remote Host
```

even when Office PC itself is not Remote-enabled.

### Team

Team desktop machines can both Host and Client:

```text
Alice PC UI
→ Bob MacBook Host

Bob MacBook UI
→ Alice PC Host
```

This is user-driven remote control, **not autonomous host-to-host orchestration**.

---

## 18. Pro Team Membership

Team users use separate accounts. Do not encourage shared credentials.

Roles:

```text
Owner
Admin
Member
```

### Owner

- billing;
- delete Space;
- transfer ownership;
- manage members;
- manage Hosts.

### Admin

- invite/remove members;
- add/remove Hosts;
- manage ordinary Space configuration.

### Member

- access Team Hosts;
- create/use shells;
- add their own Host to the Team Space;
- use Remote normally.

No per-folder or per-shell ACL in Team v1.

---

## 19. Team Resource Semantics

A Host belongs to the Space.

A shell operationally belongs to its Host/Space.

If Bob creates:

```text
Bob MacBook
~/src/foo
Claude
```

Alice, as a Team Member, may see and control it.

There is no default concept of:

```text
Bob owns this shell privately.
```

If a machine should remain private, do not attach that Host to the Team Space.

---

## 20. Remove Member

Removing Bob revokes his Team Space membership immediately.

Bob's device identities may remain valid for other Spaces but lose access to this Team Space.

Hosts Bob previously attached to the Team Space do **not** automatically disappear.

Host removal is a separate resource decision.

---

## 21. Remove Host

Removing a Host from a Space means:

- Remote trust is revoked;
- Team members can no longer connect to it;
- local MultiShell remains functional;
- local shells keep running;
- project files are untouched;
- CLI histories are untouched.

Cloud entitlement must never destroy local work.

---

## 22. Team Invite Model

V1 supports:

- invite by email;
- or single-use expiring invite link.

Also support:

```text
Pending invites
Revoke invite
```

Do not ship reusable public invitation URLs initially.

---

## 23. Solo Host Slot

Solo has exactly:

```text
1 active Remote Host
```

Not one client.

Not one device.

One **HostMembership** with active Remote entitlement.

---

## 24. Solo Host Transfer

Host transfer intentionally includes friction to discourage account sharing.

Use the wording:

```text
Transfer Remote Host
```

not:

```text
Switch Host
```

It should feel like a license/trust transfer, not daily navigation.

---

## 25. Solo Transfer — Normal Path

If the current Host is reachable:

```text
Current
Home PC ● Online

New
Office PC

Transfer Remote Host
```

Requirements:

1. recent account re-authentication;
2. current Host directly approves the transfer;
3. old Host loses Remote entitlement;
4. new Host becomes the active Remote Host.

If the current Host approves, transfer may be immediate.

This keeps legitimate planned hardware migration usable.

---

## 26. Solo Transfer — Recovery Path

If the current Host is unavailable or lost:

```text
Home PC
○ Offline

Recover Solo Host
```

Requirements:

- recent IdP reauthentication;
- passkey/strong account verification where available;
- recovery delay.

Baseline:

```text
24-hour activation delay
```

During the delay:

```text
Office PC
Pending Solo Host recovery
Available in ...
```

If the old Host returns, recovery may be cancelled or directly approved.

---

## 27. Solo Transfer Cooldown

After Host recovery/transfer:

> further transfer to a different Host is cooldown-protected unless the current Host itself approves.

Baseline direction:

```text
7-day cooldown
```

Direct current-Host approval may bypass the cooldown.

Purpose:

- legitimate planned migration stays practical;
- account-sharing rotation becomes annoying;
- no artificial annual transfer quota is needed.

---

## 28. What Does Not Count as Host Transfer

Do not treat these as a new Host:

- normal application update;
- connection-key rotation;
- normal Host key refresh.

Stable Device identity should survive ordinary upgrades.

A clean OS reinstall may create a new Device identity and use Host Recovery.

Do not use invasive hardware fingerprinting to secretly bind devices.

---

## 29. Why Solo Discourages Sharing

Sharing one Solo login does not create multiple useful remote machines.

At any moment only:

```text
1 Remote Host
```

can be active.

Constant Host swapping triggers:

- reauthentication;
- direct approval requirements;
- recovery delay;
- cooldown.

Genuine multi-Host users should choose Team.

---

## 30. Why Team Encourages Sharing

Team intentionally makes collaboration easy.

Flat pricing means:

```text
Invite Bob
```

does not immediately change the bill.

Team should feel like:

> Add your team and your Hosts.

not:

> Count seats before every invite.

Backend still applies fair-use protection to obvious abuse.

---

## 31. UX Mental Model

Shared top-level concepts:

```text
Home
Space
Device
Folder
Shell
Terminal
Activity
Settings
```

Do not introduce overlapping concepts such as:

```text
Workspace
Agent
Session
Project database
```

unless truly necessary.

---

## 32. Desktop UX

Desktop local opens directly into work, not a SaaS dashboard.

Concept:

```text
┌───────────────────────────────────────────────────┐
│ MultiShell     Personal / Acme      🔔   ⌘   ⚙  │
├──────────────────┬────────────────────────────────┤
│ ▾ MultiShell     │ Kilo                ● Running │
│   ● Kilo    FULL │ D:\src\MultiShell              │
│   ● Claude       ├────────────────────────────────┤
│   ○ Codex        │                                │
│                  │           TERMINAL             │
│ ▾ Website        │                                │
│   ○ Gemini       │                                │
│                  │                                │
│ + Add Shell      │                                │
│ + New Project    │                                │
└──────────────────┴────────────────────────────────┘
```

---

## 33. Folder Sidebar

Folder group:

```text
▾ MultiShell                  +
   ● Kilo              FULL
   ● Claude
   ○ Codex
```

`+` adds another shell to the same folder.

Rows remain clean. Secondary actions use hover/context/`···`.

---

## 34. Add Shell vs New Project

### Add Shell

Adds another CLI to the current folder.

### New Project

Creates/selects a new working directory and initial shell.

Do not merge them into a vague:

```text
+ New
```

---

## 35. Phone UX

Phone Home is **shell-first**, across Hosts.

Example:

```text
Acme

RUNNING

● Kilo
  MultiShell · Alice PC

● Claude !
  Website · Bob MacBook

● Codex
  Backend · Build Linux

＋ New Project
```

Do not force:

```text
Choose Host
→ Choose Folder
→ Choose Shell
```

for normal usage.

---

## 36. Mobile Terminal

Full-screen terminal with a developer accessory row:

```text
Esc Ctrl Alt Tab ↑ ↓ ← → …
```

Possible additional keys:

```text
/
|
~
Paste
```

When a physical keyboard is active, the accessory row may collapse.

Mobile terminal quality must cover:

- IME;
- Unicode;
- selection;
- scrolling;
- orientation;
- hardware keyboard;
- reconnect;
- large paste.

---

## 37. Tablet

Portrait is close to phone.

Landscape can use:

```text
shell/sidebar
+
terminal
```

Adaptive UX, not a desktop layout merely shrunk down.

---

## 38. Web

Web layout broadly mirrors desktop.

Connection context is shown lightly:

```text
Alice PC ● Remote
```

Technical diagnostics stay under details:

```text
Direct
Latency
E2E encrypted
```

Do not expose networking jargon in the normal workflow.

---

## 39. Multi-Viewer / Single Controller

Many clients may watch one shell.

Only one client controls input/resize at a time.

Example:

```text
Claude
├ Desktop viewer
├ iPhone controller
└ iPad viewer
```

A viewer may:

```text
Take Control
```

The Host atomically changes controller ownership.

Clients do not negotiate controller state directly with each other.

---

## 40. Needs Attention

Attention is separate from runtime state.

Runtime:

```text
Stopped
Starting
Running
Stopping
```

Attention:

```text
Needs Attention
```

Example:

```text
● Claude !
Running · Needs attention
```

Do not overload process state.

---

## 41. Attention Sources

Priority:

1. structured CLI integration;
2. explicit MultiShell event/control sequence;
3. optional conservative heuristics.

V1 does not parse terminal text such as:

```text
Continue? [y/n]
```

and pretend it reliably understands approval semantics.

---

## 42. Notifications

Free:

```text
local notifications
```

Solo/Team:

```text
cloud/mobile push
```

Push example:

```text
Claude needs your attention
Website · Home PC

Open
```

Push payloads should not contain terminal transcript content.

V1 does not expose dangerous direct lock-screen actions such as Approve/Reject.

Tap flow:

```text
notification
→ biometric
→ exact Space
→ exact Host
→ exact Shell
```

---

## 43. Activity Inbox

Activity stores meaningful events:

- needs attention;
- unexpected shell exit;
- Host offline;
- Host reconnect;
- update requiring user action.

Do not log every resize, heartbeat, keypress, or ordinary shell start.

---

## 44. Account Authentication

MultiShell should not own user passwords.

Login through an identity provider:

```text
Google
Apple
GitHub
Microsoft
Passkey
```

Account recovery primarily follows IdP recovery.

Authentication provider must remain replaceable.

Current implementation preference:

```text
WorkOS AuthKit
```

but architecture must not hard-code vendor assumptions.

---

## 45. Device Identity

Every installation/browser creates a cryptographic Device identity.

Flow:

```text
generate key locally
↓
private key remains local
↓
public key registered to account
```

Secure storage:

- Windows secure OS storage;
- Apple Keychain;
- Android Keystore;
- WebCrypto/browser origin;
- Linux protected local credential storage.

The Cloud never stores device private keys.

---

## 46. Browser Is a Device

A browser profile gets its own Device identity.

Clearing browser storage creates a new Device.

Do not fingerprint the user to guess the old identity.

Old Devices may be revoked manually.

---

## 47. Device Enrollment

Normal path:

```text
Sign in
→ device joins account
```

No per-Host pairing ceremony is required when the Account already belongs to the Space.

QR/device-code flow exists for:

- headless Linux;
- awkward-login devices;
- convenience.

---

## 48. Device Approval

Not required by default.

Possible future Advanced option:

```text
Require approval for new devices
OFF
```

Personal usability comes first.

---

## 49. Lost Device / Revoke

Space/account UI exposes:

```text
My iPhone
Revoke Device
```

Revoke:

```text
key marked revoked
↓
active remote connections closed
↓
future Space access denied
```

Local functionality on that device is separate.

---

## 50. Subscription and Identity Are Separate

Cancel subscription:

```text
account remains
device identities remain
Space metadata remains
local MultiShell remains
```

Remote entitlement stops.

Resubscribe:

```text
Remote returns
```

without re-pairing every Device.

---

## 51. Cloud Architecture

MultiShell Cloud is a control plane.

### Cloudflare Workers

- API;
- device registration;
- entitlement;
- membership;
- invites;
- relay credentials;
- update metadata.

### Durable Objects

- presence;
- signaling;
- ephemeral connection coordination.

### D1

Metadata:

- accounts;
- Spaces;
- memberships;
- Devices;
- public keys;
- Host memberships;
- subscriptions;
- invites;
- entitlement state.

### TURN

Relay fallback.

### R2

Not required for Remote v1.

Possible future uses:

- releases;
- encrypted diagnostic bundles;
- optional encrypted backup.

---

## 52. Billing

Preferred Merchant of Record:

```text
Lemon Squeezy
```

Alternative:

```text
Paddle
```

Purpose:

- subscription billing;
- VAT/tax handling;
- international payments.

Billing provider must remain replaceable.

---

## 53. Abuse / Cost Control

User-facing plans remain simple.

Backend still protects infrastructure.

---

## 54. P2P-First

Preferred path:

```text
Client ↔ Host
```

Relay:

```text
Client ↔ TURN ↔ Host
```

only when required.

Better direct connectivity lowers cost automatically.

---

## 55. MultiShell Is Not a VPN

No:

- subnet routing;
- generic TCP forwarding;
- SOCKS proxy;
- exit node;
- generic UDP;
- arbitrary service tunneling.

Remote connectivity transports only MultiShell protocol.

This reduces:

- abuse;
- attack surface;
- bandwidth cost;
- complexity.

---

## 56. Short-Lived Relay Credentials

Relay credentials are:

- entitlement-checked;
- scoped;
- short-lived;
- revocable.

No permanent TURN secret is stored in client configuration.

---

## 57. Hidden Usage Metrics

Backend may track:

- relay bytes;
- concurrent sessions;
- invite rates;
- Host churn;
- member churn;
- bulk traffic;
- signaling rate;
- push rate.

These are not displayed as normal-user quotas.

They are used for:

- abuse detection;
- cost safety;
- debugging.

---

## 58. Fair Use

Solo and Team are not metered in normal UX.

Do not show:

```text
37 GB / 100 GB
```

Team flat pricing remains subject to fair use.

Extreme usage may be:

- rate-limited;
- investigated;
- Remote-suspended if clearly abusive.

Local MultiShell remains usable.

---

## 59. Bulk Traffic Separation

Protocol separates:

```text
Terminal/control
```

from:

```text
Bulk
```

Bulk includes:

- large paste;
- image paste;
- future file transfer.

Bulk may be throttled without degrading keyboard/control responsiveness.

---

## 60. Global Cost Circuit Breaker

Cloud can temporarily restrict **new relay sessions** during a runaway-cost event.

Direct sessions remain possible.

Local remains fully functional.

---

## 61. MultiShell Connect

Connection philosophy:

> **Tailscale-style connectivity, MultiShell scope.**

Properties:

- P2P-first;
- E2E encrypted;
- NAT traversal;
- relay fallback;
- outbound-only Host;
- no port forwarding;
- seamless reconnect;
- per-device identity.

User-facing status may show:

```text
Online
Direct
Relay
```

only when useful.

---

## 62. Transport Strategy

V1:

```text
WebRTC DataChannel
```

because it spans:

- browser;
- iOS;
- Android;
- desktop.

Future native optimization may prototype:

```text
tailcat / magicsock / WireGuard-style
```

Transport must remain replaceable.

---

## 63. Remote Protocol Invariants

1. **Host is source of truth.**
2. **Connection does not own shell.**
3. **Protocol is transport-independent.**

---

## 64. Remote Protocol Channels

One physical connection multiplexes logical channels.

### Control

- CreateShell;
- Start;
- Stop;
- Restart;
- ChangeApproval;
- CreateDirectory;
- CloneRepository;
- AttachTerminal;
- TakeControl.

### Events

Host pushes:

- shell state changes;
- attention;
- Host state;
- controller changes;
- notification events.

### Terminal

- output;
- input;
- paste;
- resize.

Terminal is reliable and ordered.

### Bulk

- large clipboard;
- image payload;
- future file operations.

---

## 65. Protocol Encoding

Control/events:

```text
Protocol Buffers
```

Terminal hot path:

```text
small binary header
+
raw UTF-8 VT payload
```

Do not encode every terminal chunk as JSON.

---

## 66. Handshake

Exchange:

```text
AppVersion
ProtocolMin
ProtocolMax
Capabilities
Device identity context
Space context
```

Capabilities may include:

```text
terminal
filesystemBrowse
gitClone
attention
imagePaste
sitePreview
```

Feature visibility depends on capabilities, not exact product-version equality.

---

## 67. Idempotency

Mutating commands carry:

```text
requestId
```

Host deduplicates retries.

Examples:

```text
Stop stopped shell → OK
Start running shell → OK
```

Network retries must not duplicate lifecycle work unexpectedly.

---

## 68. Host State Revision

Events include a monotonic state revision.

If a client detects a gap:

```text
GetSnapshot
```

No CRDT is required.

---

## 69. Terminal Replay

Host assigns sequence numbers to terminal output.

Client remembers:

```text
lastSequence
```

Reconnect:

```text
Attach(shellId, afterSequence)
```

Host maintains a bounded in-memory raw-output ring buffer.

No disk transcript by default.

No Cloud terminal logging.

---

## 70. Replay Gap Too Large

If the buffer no longer contains the needed output:

```text
ReplayUnavailable
```

Client resets its renderer and replays available recent output.

It may inform the user:

```text
Earlier terminal output is no longer available.
```

Do not implement a second VT emulator inside Host merely for perfect restoration.

---

## 71. Remote Filesystem

Remote filesystem capability is intentionally thin.

Needed:

- list directories;
- drives/volumes;
- stat path;
- mkdir;
- select cwd;
- Git clone.

Not needed:

- rich file previews;
- code editor;
- general file manager;
- thumbnails.

---

## 72. New Project

Local/Remote flow:

```text
Space
Host
Location
Name
Source: Empty / Git Clone
Initial shell
Approval
Create & Open
```

Host uses:

- Host filesystem;
- Host Git;
- Host credentials.

Cloud receives no repository contents.

---

## 73. Host Architecture

Target:

```text
Desktop / Web / Mobile
          │
      protocol
          │
          ▼
    MultiShell Host
          │
  ┌───────┼─────────┐
  │       │         │
Shell   Files     PTY runtime
State   /Git
```

Desktop UI no longer owns shell lifetime.

---

## 74. Host Runs as User

Windows:

```text
per-user startup
```

macOS:

```text
LaunchAgent
```

Linux:

```text
systemd --user
```

Not SYSTEM/root.

This preserves:

- PATH;
- HOME;
- SSH configuration/keys available to the user;
- Git credentials;
- CLI authentication;
- user configuration.

---

## 75. Local IPC

Windows:

```text
Named Pipe
```

macOS/Linux:

```text
Unix Domain Socket
```

Local and Remote use the same business semantics.

Only transport changes.

---

## 76. State Ownership

Host owns:

- shell definitions;
- runtime state;
- cwd;
- preset;
- approval;
- processes;
- PTYs;
- replay buffers;
- controller state.

Client owns:

- theme;
- window position;
- sidebar width;
- presentation preferences;
- last selected view.

---

## 77. Host Lifetime

UI closes:

```text
Host lives
Shells live
Remote lives
```

UI crashes:

```text
Host lives
Shells live
```

Host crashes:

```text
managed shells stop
```

Logout/reboot:

```text
Host stops
Shells stop
```

MultiShell 2.0 does not initially promise persistent PTY across Host crash/reboot.

---

## 78. Shell Runtime Interface

Concept:

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

Implementations:

```text
WindowsTerminalConptyRuntime
PosixPtyRuntime
```

Future:

```text
DaemonBackedRuntime
```

may be added without rewriting shell business logic.

---

## 79. Windows Runtime

Keep the current proven stack:

```text
Windows Terminal native control
ConPTY
ConptyConnection
Job Object
native clipboard
native input
```

The existing native patch already exposes bridge primitives such as:

```text
MultiShellConptyCreate
MultiShellConptyWrite
MultiShellConptyResize
MultiShellConptyTerminate
MultiShellConptyDestroy
```

plus renderer/input bridge functionality.

Use these instead of building a new ConPTY infrastructure.

---

## 80. Windows PTY Migration

### Phase A — same process

Split renderer/input from PTY while still inside `MultiShell.exe`.

Input remains:

```text
keyboard/IME
→ native HwndTerminal
→ native input logic
→ write callback
→ PTY
```

Paste remains native.

No WPF fake typing.

No managed bracket simulation.

### Phase B — Host process

Only after all native tests pass:

```text
WPF renderer
    │
Named Pipe
    │
Host-owned ConPTY
```

Then this becomes required:

```text
kill UI
→ shell survives
```

---

## 81. Platform Stack

### Host/Core

```text
C# / .NET
```

### Windows Desktop

```text
WPF
+
patched native Windows Terminal
```

### Web

```text
React
TypeScript
Vite
xterm.js
```

### macOS/Linux Desktop

Preferred:

```text
Tauri 2
React
xterm.js
```

### iOS/Android

```text
Expo / React Native
```

Use native outer UI with a focused xterm.js WebView for the terminal.

---

## 82. Mobile Prototype Quality Gate

Must test:

- Vietnamese;
- IME;
- Unicode;
- iOS keyboard;
- Android keyboard;
- iPad physical keyboard;
- Ctrl/Alt;
- large paste;
- selection;
- scrolling;
- orientation;
- background/resume;
- reconnect.

Do not assume xterm.js automatically solves terminal UX.

---

## 83. Notifications and Push

Push uses Cloud because suspended mobile apps cannot rely on persistent WebRTC.

Flow:

```text
Host attention
→ Cloud
→ APNs / FCM
→ mobile
```

Tapping establishes or re-establishes the Remote connection.

---

## 84. Security Summary

Security protects:

```text
identity
device trust
Space membership
transport
relay credentials
updates
```

It does not attempt to protect:

```text
the user from their own shell
```

Important principles:

- device private keys stay local;
- Remote traffic is encrypted;
- Cloud does not store terminal transcripts by default;
- Host runs as the OS user;
- no automatic elevation;
- no generic network tunneling.

---

## 85. Update Model

Principle:

> **A running shell is more important than an immediate update.**

An ordinary update must never silently stop live shells.

---

## 86. Version Compatibility

UI and Host may use different product versions.

Example:

```text
UI 2.5
Host 2.3
```

They work when their protocol ranges overlap.

Protocol version and product version are separate.

Capabilities handle incremental feature differences.

---

## 87. Side-by-Side Runtime

Portable layout:

```text
MultiShell/
├ MultiShell.exe
├ shells.json
├ preferences.json
└ runtime/
   ├ 2.4.0/
   └ 2.5.0/
```

A stable launcher selects the runtime.

---

## 88. Update While Shell Runs

Default:

```text
download new version
→ stage
→ keep current Host alive
→ install/restart Host when idle
```

UI may update separately if protocol-compatible.

Explicit user action:

```text
Restart shells and update now
```

is allowed.

---

## 89. Rollback

Keep:

```text
current
previous
```

runtime.

If a new Host repeatedly fails startup:

```text
automatic rollback
```

---

## 90. Update Security

Packages should include:

- version;
- platform;
- architecture;
- hash;
- signature.

Use platform signing:

- Windows Authenticode;
- Apple signing/notarization;
- signed Linux release artifacts.

---

## 91. Persistence

`shells.json` remains simple.

Host becomes its sole writer.

2.x rule:

> **The schema evolves additively.**

Do not needlessly replace it with a large database.

---

## 92. Atomic Persistence

Write:

```text
shells.json.tmp
→ flush
→ atomic replace
```

Keep:

```text
shells.json.bak
```

If corruption occurs:

- preserve the original;
- do not overwrite it with empty state;
- try the backup;
- surface recovery to the user.

---

## 93. UI Crash Recovery

```text
UI dies
→ Host survives
→ shells survive
```

Restart:

```text
connect Host
→ GetSnapshot
→ reattach terminal
```

No resume prompt is needed because the process never stopped.

---

## 94. Host Crash Recovery

```text
Host crashes
→ Job Objects close
→ managed processes stop
```

Host restart:

```text
definitions remain
runtime = Stopped
```

User may Start/Resume using the CLI's native recovery semantics.

This is accepted for 2.0.

---

## 95. Reboot / Logout

Same semantics as Host shutdown.

Do not automatically relaunch every previously running AI CLI by default.

---

## 96. Remote Host Update

A headless Host can:

```text
download update
stage update
install when idle
```

Remote client may request:

```text
Update when idle
```

without physical access to the Host.

---

## 97. Failure Invariant

Cloud outage or subscription problems must not break local MultiShell.

Always:

```text
Local shell management ✅
Remote Cloud may be unavailable
```

---

## 98. Learn From CodeShellManager

Use it as inspiration for:

- information hierarchy;
- shell grouping;
- status visibility;
- settings structure;
- visual maturity.

Do not copy:

- dense global toolbar;
- giant feature surface;
- grid-first philosophy.

---

## 99. Learn From 9Remote

Study:

- persistent PTY daemon boundary;
- mobile developer key row;
- biometrics;
- mobile multi-host UX;
- direct/relay networking;
- site preview;
- background operation.

Do not copy:

- Node.js Host architecture;
- filesystem-jail philosophy;
- generic remote-computer framing;
- remote desktop;
- full file/editor suite;
- shared master-key/no-account model.

---

## 100. Future High-Value Feature — Site Preview

After Remote terminal quality is excellent:

```text
npm run dev
localhost:5173

[Open Preview]
```

Phone can view the Host-local development server through the MultiShell connection.

Useful for remote/vibe-coding workflows.

Not an MVP blocker.

---

## 101. Explicit Non-Goals

Do not build in the 2.0 MVP:

- grid/split as the core paradigm;
- editor;
- full file manager;
- Git GUI;
- remote desktop;
- agent orchestration;
- agent-to-agent automatic control;
- workflow graph;
- VPN;
- generic port forwarding;
- Team per-folder ACL;
- enterprise RBAC;
- marketplace;
- cloud AI execution;
- repository cloud storage;
- terminal transcript cloud storage;
- persistent PTY through Host crash/reboot;
- multiple same-preset shells in one folder.

---

## 102. Implementation Roadmap

### Stage 0 — MultiShell 1.x Polish

Deliver:

- logical command resolution;
- hide `.cmd/.ps1` complexity;
- manual executable only under Advanced;
- cleaner visual hierarchy;
- preserve terminal behavior.

Gate:

> existing Windows terminal regressions all pass.

### Stage 1 — Core Extraction

Create:

```text
MultiShell.Core
MultiShell.Protocol
```

Move:

- shell model;
- preset model;
- lifecycle rules;
- uniqueness rules;
- launch intent.

Do not change terminal behavior.

### Stage 2 — Host / Protocol Skeleton

Create:

```text
MultiShell.Host
```

Host owns:

- shell definitions;
- persistence;
- authoritative metadata.

Add:

- local IPC;
- handshake;
- request/event model;
- snapshots.

WPF stops directly owning `shells.json`.

### Stage 3A — External PTY, Same Process

Decouple:

```text
renderer/input
↕
PTY runtime
```

inside the same process.

Use native input/output bridges.

No network yet.

Gate:

- Vietnamese;
- IME;
- 1 MB paste;
- bracketed paste;
- image paste;
- scroll;
- copy;
- mouse;
- resize;
- TUI;
- multi-session isolation.

Any regression = stop.

### Stage 3B — Host-Owned PTY

Move PTY runtime into MultiShell Host.

WPF only renders/input.

Gate:

```text
kill MultiShell UI
→ Host lives
→ shells live

restart UI
→ shells reappear
→ terminal reattaches
```

### Stage 4 — LAN Remote Prototype

Browser client:

- GetSnapshot;
- list shells;
- Attach;
- output/input;
- resize;
- Start/Stop/Restart;
- Take Control;
- reconnect;
- replay.

No full SaaS yet.

### Stage 5 — Production Transport

Add:

- WebRTC;
- direct P2P;
- NAT traversal;
- TURN fallback;
- reconnection;
- diagnostics.

Gate:

```text
Wi-Fi → 5G
```

must not destroy the shell or require session restart.

### Stage 6 — Cloud Identity / Space Model

Add:

- Accounts;
- Spaces;
- Device keys;
- Host memberships;
- Solo/Team entitlement;
- presence;
- signaling;
- revoke;
- invites;
- relay credentials.

### Stage 7 — Remote Workspace Beta

Web product supports:

- shell-first Home;
- multi-Space;
- multi-Host;
- remote filesystem picker;
- New Project;
- mkdir;
- clone;
- lifecycle;
- control ownership.

Do not charge until continuity quality is sufficient.

### Stage 8 — Mobile

iOS/Android:

- biometric;
- secure keys;
- mobile shell Home;
- terminal;
- special key row;
- push;
- deep links;
- QR enrollment.

### Stage 9 — Pro Solo Launch

Launch once this loop is robust:

```text
desktop shell
→ leave desk
→ push
→ phone
→ same shell
→ respond
→ return desktop
```

Pricing baseline:

```text
$5/month
$48/year
1 Remote Host
Unlimited clients
```

### Stage 10 — Pro Team

Add:

- shared Space;
- member invitations;
- Owner/Admin/Member;
- unlimited Hosts;
- desktop-to-desktop Remote;
- flat Team subscription;
- member/Host revocation.

Pricing baseline:

```text
$20/month
$192/year
```

### Stage 11 — macOS/Linux

Add POSIX Host runtime and desktop clients.

Keep the same protocol and shell semantics.

### Stage 12 — Expansion

Potential:

- Site Preview;
- structured approval integrations;
- better attention integrations;
- native tailcat/magicsock experiments;
- persistent runtime daemon if justified;
- external individual Host sharing in the spirit of Tailscale machine sharing.

---

## 103. Final Core Invariants

1. **Host is source of truth.**
2. **Connection never owns shell.**
3. **Protocol is transport-independent.**
4. **Windows native terminal fidelity must not regress.**
5. **No managed fake typing/paste.**
6. **UI crash must not stop shells.**
7. **Host crash may stop shells in 2.0.**
8. **Updates never silently stop live shells.**
9. **UI/Host version mismatch is acceptable when protocol-compatible.**
10. **`shells.json` remains additive through 2.x.**
11. **Cloud is control plane, not shell runtime.**
12. **Cloud stores no terminal transcript by default.**
13. **Remote user acts with Host OS-user authority.**
14. **Local functionality is independent of subscription/cloud.**
15. **Device count is not a billing metric.**
16. **Host entitlement determines which machines are remotely accessible.**
17. **Pro Solo has exactly one active Remote Host.**
18. **Solo Host Transfer contains deliberate anti-sharing friction.**
19. **Pro Team uses separate Accounts inside one shared Space.**
20. **Team supports unlimited Remote Hosts under fair use.**
21. **Desktop clients may control Remote Hosts according to Space entitlement.**
22. **Desktop-to-desktop Remote is user-driven, not autonomous Host orchestration.**
23. **MultiShell remains a shell workspace, not an IDE/VPN/remote-desktop platform.**

---

## 104. Final Product Definition

> **MultiShell is a local-first workspace for running and continuing coding shells across your computers and devices, with native local terminal quality and secure seamless remote continuity.**

Plan positioning:

> **Free manages your shells locally.**

> **Pro Solo brings one personal Host to every device you use.**

> **Pro Team turns multiple people and machines into one shared shell workspace.**

Commercial differentiation:

> **9Remote lets you remote into a computer. MultiShell lets you continue the shell work you already live in.**
