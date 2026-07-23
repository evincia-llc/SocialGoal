# Decision log

Append-only. IDs are stable; flip Status when the operator decides. Code must not
depend on an OPEN decision. D1-D9 originate in the epic doc's decision register.

## Decided

### D0 -- Modernization target: single epic to .NET 10 (no 4.8.x stopover)

- **Status:** DECIDED · 2026-07-23 · Owner: Jerry
- **Context:** Options were (a) trimmed two-epic (safety net + mechanical 4.8.x
  retarget, then Core), (b) full 4.8.x modernization epic then Core, (c) single
  epic to .NET 10. LMRR Phase 1 targets .NET 10 directly; no live users/data to
  keep supported on Framework in the interim.
- **Decision:** Single epic straight to .NET 10 following LMRR Phases 0-3.
  One auth migration (Identity 1.0 -> ASP.NET Core Identity directly).
- **Consequence:** No intermediate stable Framework platform; the Phase 0 safety
  net and the Sprint 5 spikes are the risk controls in its place.

### D1 -- No live deployment, database, or users

- **Status:** DECIDED · 2026-07-23 · Owner: Jerry
- **Decision:** There are no active users, no production database, and no live
  deployment. Only the .sln/codebase exists; the schema of record is generated
  from the EF6 Code First model (Sprint 2 snapshot).
- **Consequence:** The live-data rigor track drops away (real-row hash proofs,
  reconciliation runs, delta cutover, Sprint 14 freeze/canary). Identity hash
  compatibility is proven against seeded users only. The LMRR trigger question
  closes by construction (no pre-existing DB to drift), documented in Sprint 2.

### D2 -- Hosting: Azure, private (non-public) access

- **Status:** DECIDED · 2026-07-23 · Owner: Jerry
- **Decision:** Host on Azure, internally/not publicly. Default shape: Azure
  App Service (Linux) with Entra ID authentication (Easy Auth) plus IP
  restrictions -- no anonymous public surface. Default `*.azurewebsites.net`
  hostname; operator's domains stay in reserve. The stronger VNet/Private
  Endpoint posture is a deliberate option at Sprint 14 hardening, not the POC
  default (adds VPN/Bastion + higher tier for little POC value).
- **Consequence:** Sprint 5's decision gate is cleared. Cascading defaults now
  concrete: D8 -> Application Insights; D9 -> Azure Blob Storage (also Data
  Protection key store); deployment via GitHub Actions with OIDC federated
  credentials (no publish-profile secrets in the repo). Exact Easy Auth/IP
  wiring is proven during the Sprint 5 host spike.

### D10 -- Sprint-scale branching and large PRs

- **Status:** DECIDED · 2026-07-23 · Owner: Jerry
- **Context:** Solo owner/operator, POC, no production environment. Many small
  feature branches/PRs would be ceremony without an audience.
- **Decision:** No PR-per-work-item/task/story ceremony. PR granularity is
  pragmatic: a coherent chunk of work -- a sprint, a workstream within a sprint,
  or a slice -- sized by what reviews sensibly (`sprint/s<n>-<slug>` naming).
  Large PRs are acceptable when the work is coherent; small PRs remain fine.
- **Consequence:** Master protection, guard hooks, Copilot iterate-until-clean,
  and security-reviewer passes all stay. Commit hygiene inside branches matters
  more (bisect/rollback granularity moves to commit level). On large diffs
  Copilot depth thins; the security-reviewer agent carries more weight in
  Phase 2.

## Open (blocking noted per epic)

| ID | Decision | Default recommendation | Blocks |
|---|---|---|---|
| D3 | Image URL import: remove or harden | Remove | Sprint 11 |
| D4 | `SearchController`: authorize or documented-public | Add `[Authorize]` | Sprint 11 |
| D5 | External logins | Remove dead Google OpenID; add Google OAuth 2.0 only if wanted | Sprint 8 |
| D6 | Bootstrap 3.4.1 parity vs Bootstrap 5 rebuild | 3.4.1 parity this epic; BS5 as follow-on | Sprint 12 |
| D7 | Must email flows actually send? | No-op mailer with logging unless invites/reset required | Sprint 13 |
| D8 | Observability backend (App Insights vs OTel+APM) | Application Insights (per D2 = Azure); confirm at Sprint 13 | Sprint 13 |
| D9 | Profile images: local disk vs object storage | Azure Blob Storage (per D2 = Azure); confirm at Sprint 11 | Sprint 11 |
