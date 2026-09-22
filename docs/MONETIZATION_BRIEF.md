# MultiShell Monetization Brief

**Status:** Approved monetization baseline  
**Purpose:** Source of truth for pricing, packaging, monetization roles, cost-control rules, and paywall philosophy.  
**Relationship to architecture:** This brief supersedes the pricing/packaging assumptions in `docs/ARCHITECTURE_2_0.md` wherever they conflict. The architecture, security, Host, protocol, and terminal invariants remain unchanged.

---

## 1. Monetization Goal

MultiShell should not depend on a single customer type or a single pricing mechanism.

The business should have three revenue engines with different economics:

1. **Personal** — low profit per user, very large potential scale.
2. **Team** — medium profit, predictable recurring revenue, lower churn.
3. **Vibe** — high-margin convenience product for non-technical / vibe-coding users.

The portfolio should be intentionally balanced:

```text
Personal → volume
Team     → stable MRR
Vibe     → margin
```

MultiShell should not pivot into enterprise complexity before real demand appears.

No current need for:

- Enterprise plan;
- Contact Sales;
- SAML/SCIM;
- MDM/device posture;
- SIEM;
- complex RBAC;
- procurement-heavy security features;
- per-seat enterprise sales motion.

The target market is primarily:

- individual developers;
- prosumers;
- small technical teams;
- agencies;
- startup teams;
- non-technical users learning or practicing vibe coding.

---

## 2. Pricing Structure

### Free

```text
$0
```

Role:

> Acquisition and product habit formation.

Includes:

- unlimited local use;
- unlimited local folders;
- unlimited local shells;
- all local CLI presets;
- Standard / Full Approval;
- local notifications;
- one Remote Host;
- unlimited client devices;
- direct P2P Remote;
- web/mobile/desktop clients where available;
- limited managed relay allowance;
- basic Remote shell access.

Does not include the premium convenience layer:

- reliable managed relay fair use;
- mobile push;
- Needs Attention push;
- premium Remote New Project flow;
- Vibe workflows;
- Team Space.

Free should expose enough Remote functionality for users to experience the core magic:

```text
desktop shell
→ leave desk
→ open phone
→ same shell
```

Free must not become a large infrastructure subsidy.

---

## 3. Personal Plus

### Pricing baseline

```text
$2.99/month
$24/year
```

Role:

> Low-margin, high-scale recurring revenue.

Target:

- individual technical users;
- developers;
- power users;
- people who already understand terminals and coding CLIs.

Includes:

- one active Remote Host;
- unlimited client devices;
- direct P2P Remote;
- managed relay under fair use;
- mobile push notifications;
- Needs Attention push;
- Remote New Project;
- remote filesystem picker;
- large paste/image convenience;
- full Remote continuity experience.

Personal Plus is intentionally cheap.

The goal is not maximum ARPU. The goal is:

> A price low enough that legitimate users prefer paying instead of sharing accounts or building their own workaround.

---

## 4. Personal Host Entitlement

Personal Free and Personal Plus both use:

```text
1 active Remote Host
```

Client devices are not limited.

Examples of clients:

- another Windows desktop;
- Mac desktop;
- browser;
- iPhone;
- iPad;
- Android.

A desktop may act purely as a client without consuming the Remote Host slot.

Host entitlement limits:

> which machine may be accessed remotely

not:

> which machine may act as a client.

---

## 5. Solo Host Transfer Anti-Sharing Rules

The one-Host model is the primary anti-account-sharing mechanism.

Use:

```text
Transfer Remote Host
```

not:

```text
Switch Host
```

Normal transfer path:

1. recent re-authentication;
2. current Host approves;
3. old Host loses Remote entitlement;
4. new Host becomes active;
5. transfer may complete immediately.

Recovery path if current Host is unavailable:

- strong re-authentication;
- recovery flow;
- baseline 24-hour activation delay.

Cooldown direction:

```text
7 days
```

unless the current Host directly approves another transfer.

Do not use invasive hardware fingerprinting.

Do not use a hard yearly transfer quota.

Do not punish legitimate:

- application updates;
- key rotation;
- normal device maintenance.

---

## 6. Team

### Pricing baseline

```text
$39/month
includes 5 members
```

Additional members:

```text
+$6/member/month
```

Role:

> Stable recurring revenue engine.

Examples:

| Team size | Monthly price |
|---:|---:|
| 2–5 | $39 |
| 6 | $45 |
| 10 | $69 |
| 20 | $129 |

Team pricing is based on people, not Host count.

Includes:

- shared Team Space;
- 5 included members;
- invite/remove members;
- unlimited Remote Hosts under fair use;
- unlimited client devices;
- desktop-to-desktop Remote;
- web/mobile Remote;
- managed relay;
- push;
- Needs Attention;
- Remote New Project;
- shared shell visibility;
- member/Host revocation.

Team should remain simple.

Do not add enterprise-style permission systems until real users demand them.

Baseline Team mental model:

> A Host added to the Team Space is accessible to Team members.

A private personal machine should remain in the Personal Space.

---

## 7. Why Team Uses Base + Members

Do not use the old model:

```text
$20 flat
unlimited members
```

That creates poor value capture and dangerous economics.

Do not make early Team pricing feel like enterprise procurement either.

The preferred model:

```text
$39
includes 5 members
```

creates:

- meaningful minimum recurring revenue;
- low friction for small teams;
- room to invite colleagues without thinking about every seat;
- predictable expansion revenue after five members;
- no Host-count anxiety.

Team is expected to have:

- higher retention than Personal;
- lower churn;
- more predictable monthly revenue.

---

## 8. Vibe

### Pricing baseline

```text
$14.99/month
$119/year
```

Working product name:

```text
MultiShell Vibe
```

or another future consumer-facing name.

Role:

> High-margin convenience product for non-technical and early-stage vibe-coding users.

Vibe must not simply be the same Personal product sold at a higher price.

It must provide a materially more guided workflow.

---

## 9. Vibe Customer

Typical user does not want to learn:

- PATH;
- shell extensions;
- Git setup;
- npm commands;
- cwd;
- ports;
- localhost;
- CLI flags;
- terminal setup details.

They want:

> I want to build an app with Claude/Kilo/Codex and see it running.

Vibe monetizes the abstraction layer around the same MultiShell Core.

---

## 10. Vibe Must Not Bundle Expensive Compute

Do not bundle:

- LLM tokens;
- GPUs;
- cloud containers;
- AI inference;
- managed databases;
- heavy hosting.

Users continue paying the AI/CLI provider directly.

Examples:

- Claude;
- OpenAI/Codex;
- Gemini;
- Kilo/provider API.

MultiShell Vibe sells:

> orchestration, setup, workflow, safety, and convenience.

This keeps gross margin high and infrastructure cost low.

---

## 11. Vibe Hero Features

### 11.1 Guided Create App Flow

Instead of exposing raw setup:

```text
New Folder
→ Git clone
→ Add shell
→ install dependencies
→ run dev server
```

Vibe can offer:

```text
What do you want to build?

○ Website
○ Web app
○ Mobile app
○ Existing GitHub project

Choose AI
Claude / Kilo / Codex

[ Start Building ]
```

The underlying Core remains unchanged.

---

### 11.2 Setup Doctor

Detect common requirements:

```text
Git       ✓
Node      ✓
Claude    ✗
npm       ✓
GitHub    ✓
```

Then provide guided resolution.

Examples:

```text
Claude is missing
[ Show me how to install it ]
```

or, only when safe and maintainable:

```text
[ Install ]
```

Setup friction that is trivial for experienced developers can be a major blocker for non-technical users.

---

### 11.3 Project Recipes

Examples:

- Next.js;
- Vite + React;
- Expo;
- static website;
- existing GitHub repository.

Recipes orchestrate existing primitives:

```text
create/clone folder
→ bootstrap
→ open shell
→ launch agent
```

Do not turn MultiShell into an IDE.

---

### 11.4 Site Preview

Site Preview is a high-value Vibe feature.

Example:

```text
localhost:5173 detected

Your app is running

[ Open Preview ]
[ Open on Phone ]
```

The technical primitive may be simple, but perceived value is high for non-technical users.

---

### 11.5 Human-Language State

Developer UI may show:

```text
Claude
● Running
FULL
```

Vibe can translate structured state into:

```text
Claude is working on your app
```

or:

```text
Claude needs an answer
```

or:

```text
Your preview stopped
Restart it?
```

Do not invent fake AI interpretation. Use actual structured MultiShell state.

---

### 11.6 Safety Checkpoints

Vibe users may need guardrails.

Example:

```text
You have changes that are not committed.

Create a safety checkpoint first?

[ Create checkpoint ]
[ Continue anyway ]
```

Prefer local Git-based checkpoints.

Do not require expensive cloud backup.

---

### 11.7 Fix My Setup

A diagnostic workflow can inspect:

```text
Git
Node
PATH
CLI
project folder
dev server
port
```

and present a simplified result:

```text
Everything looks good
```

or:

```text
Node is not available to Claude
[ Fix PATH ]
```

This can be a meaningful retention feature for non-technical users.

---

## 12. Vibe Architecture Rule

Vibe is a workflow/UI layer over the same platform.

Architecture:

```text
                 MultiShell Core
                       │
          ┌────────────┼────────────┐
          │            │            │
      Developer UI   Team UI      Vibe UX
```

Do not fork:

- Host;
- PTY runtime;
- Remote Protocol;
- terminal implementation;
- shell lifecycle semantics.

Vibe-specific logic should orchestrate shared primitives.

---

## 13. Fair Paywall Philosophy

Do not charge Vibe users more for the exact same experience simply because they know less.

Charge for real convenience:

- guided workflows;
- recipes;
- Setup Doctor;
- Site Preview UX;
- automatic structured setup;
- safety checkpoints;
- simplified status;
- diagnostic/fix flows.

Example of fair differentiation:

Free/Personal:

```text
localhost:5173 detected
```

Vibe:

```text
Your app is ready

[ Preview ]
[ Open on Phone ]
```

The underlying capability is not maliciously hidden; the paid product packages it into a higher-value workflow.

No dark patterns.

No misleading claims.

No fake urgency.

No intentionally confusing cancellation.

---

## 14. Optional One-Time Vibe Revenue

Future option:

### Vibe Starter Packs

Possible pricing:

```text
$19–49 one-time
```

Examples:

- Launch a SaaS;
- Personal website;
- AI chatbot;
- internal dashboard;
- Mobile MVP.

Pack may include:

- project recipe;
- recommended CLI;
- starter prompts;
- environment checks;
- run commands;
- preview configuration;
- deployment checklist.

Do not build a marketplace before demand exists.

---

## 15. Optional Affiliate Revenue

If legitimate vendor referral programs exist, Setup flows may use disclosed affiliate links.

Potential categories:

- AI coding tools;
- hosting;
- domains;
- databases;
- deployment services.

Rules:

1. Clearly disclose referral relationships.
2. Do not make commissions determine technical recommendations.
3. Do not hide better free options solely because they pay no affiliate fee.
4. Affiliate revenue is ancillary, not a core dependency.

---

## 16. Free Cost Control

Free must remain scalable even without large funding.

### Direct first

Preferred:

```text
Client ↔ Host
```

Cloud handles only:

- identity;
- presence;
- signaling;
- entitlement.

### Limited managed relay

Free gets a bounded relay allowance.

Exact allowance should be chosen from production telemetry, not ideology.

Initial conceptual target may be around:

```text
~1 GB relay/account/month
```

but this is not a permanent product promise until cost data exists.

### Bulk separation

Keep bulk traffic separate from control/terminal traffic.

Bulk includes:

- huge paste;
- images;
- future file transfer;
- Site Preview payloads.

Do not allow Free to become a generic bandwidth service.

### Existing private network

Advanced users may connect over:

- LAN;
- existing Tailscale/private network;
- other directly reachable private paths.

MultiShell does not need to pay relay cost when users already have connectivity.

---

## 17. Managed Relay Philosophy

Personal Plus, Team, and Vibe may receive managed relay under fair use.

Do not expose ordinary bandwidth meters.

Do not price per terminal hour.

Do not price per gigabyte.

Backend may still monitor:

- relay bytes;
- bulk traffic;
- concurrency;
- anomalous transfer behavior.

Cost controls should stay mostly invisible during normal use.

---

## 18. Revenue Roles

### Personal

Characteristics:

```text
low ARPU
large funnel
low support
large potential scale
```

Purpose:

> Distribution and volume.

### Team

Characteristics:

```text
medium ARPU
lower churn
predictable MRR
expansion revenue
```

Purpose:

> Business stability.

### Vibe

Characteristics:

```text
high ARPU relative to infrastructure
high gross margin potential
higher support
higher trend sensitivity
possibly higher churn
```

Purpose:

> Margin.

The three lines intentionally balance one another.

---

## 19. Desired Revenue Portfolio

This is not a forecast, only a strategic shape.

Example mature mix:

```text
Personal
large share of users
smaller share of revenue

Team
smaller share of users
largest stable revenue contribution

Vibe
meaningful user share
high-margin contribution
```

Do not optimize early decisions toward a fixed percentage split.

Observe actual conversion and churn.

---

## 20. Funnel

### Developer funnel

```text
discover MultiShell
↓
Free Local
↓
try 1-Host Remote
↓
use daily
↓
want reliable relay/push
↓
Personal Plus
```

### Team funnel

```text
developer already uses MultiShell
↓
uses it at work
↓
creates Team Space
↓
invites coworkers
↓
Team subscription
```

### Vibe funnel

```text
non-technical user wants to build an app
↓
setup friction
↓
MultiShell Vibe simplifies environment
↓
agent starts
↓
preview works
↓
user stays because setup and continuity are handled
```

---

## 21. What MultiShell Should Not Monetize

Avoid:

- bandwidth packs;
- terminal-hour packs;
- Host-count micro-tiers;
- device-count pricing for Personal;
- artificial local feature restrictions;
- AI token reselling;
- cloud compute dependency;
- generic VPN service;
- arbitrary file-transfer business;
- deliberately confusing pricing.

The strongest model is:

> local software + low-cost connectivity + high-value workflow convenience.

---

## 22. No Enterprise Push Yet

Do not proactively build:

- SAML;
- SCIM;
- advanced ACL;
- device posture;
- compliance recording;
- SIEM integration;
- procurement tooling.

Architecture should remain capable of supporting them later.

Build them only after credible customers request them.

The immediate commercial target is:

```text
individual developers
small teams
agencies
startups
vibe coders
```

not Fortune 500 procurement.

---

## 23. Pricing Summary

| Plan | Price | Primary role |
|---|---:|---|
| **Free** | **$0** | Acquisition |
| **Personal Plus** | **$2.99/mo · $24/year** | Scale |
| **Vibe** | **$14.99/mo · $119/year** | High-margin |
| **Team** | **$39/mo incl. 5 members** | Stable MRR |
| Additional Team member | **+$6/mo** | Expansion |

These are baseline launch hypotheses, not immutable forever.

Prices may change based on:

- conversion;
- churn;
- relay cost;
- support burden;
- geographic demand;
- actual customer willingness to pay.

Packaging principles matter more than exact launch numbers.

---

## 24. Final Monetization Principles

1. **Personal creates volume.**
2. **Team creates stable recurring revenue.**
3. **Vibe creates margin.**
4. **Free must be useful enough to create habit.**
5. **Free must have a bounded infrastructure cost.**
6. **Direct P2P is both a product architecture choice and a cost strategy.**
7. **Do not subsidize unlimited Free relay.**
8. **Personal pricing should be low enough to discourage account sharing through economics, not DRM.**
9. **Team pricing should capture value without enterprise complexity.**
10. **Vibe must deliver real convenience, not simply charge non-technical users more for the same product.**
11. **Do not bundle expensive AI compute into Vibe.**
12. **Do not let monetization requirements contaminate local terminal quality or Core architecture.**
13. **Local MultiShell remains usable regardless of subscription status.**
14. **Cloud cancellation never destroys local work.**
15. **Enterprise features remain demand-driven, not roadmap-driven.**

---

## 25. One-Line Strategy

> **Personal scales the user base, Team pays the bills, and Vibe monetizes convenience.**
