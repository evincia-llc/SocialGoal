# Decision log

Append-only. IDs are stable; flip Status when the operator decides. Code must not
depend on an OPEN decision. D1-D9 originate in the epic doc's "Open decisions".

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

## Open (blocking noted per epic)

| ID | Decision | Default recommendation | Blocks |
|---|---|---|---|
| D1 | Any live deployment / database / users? | Assume none (public sample); escalate rigor if wrong | Sprint 5 gate |
| D2 | Hosting target (App Service / containers / on-prem) | Needed before Sprint 5; drives D8, D9, Data Protection keys | Sprint 5 gate |
| D3 | Image URL import: remove or harden | Remove | Sprint 11 |
| D4 | `SearchController`: authorize or documented-public | Add `[Authorize]` | Sprint 11 |
| D5 | External logins | Remove dead Google OpenID; add Google OAuth 2.0 only if wanted | Sprint 8 |
| D6 | Bootstrap 3.4.1 parity vs Bootstrap 5 rebuild | 3.4.1 parity this epic; BS5 as follow-on | Sprint 12 |
| D7 | Must email flows actually send? | No-op mailer with logging unless invites/reset required | Sprint 13 |
| D8 | Observability backend (App Insights vs OTel+APM) | Follows D2 | Sprint 13 |
| D9 | Profile images: local disk vs object storage | Object storage if D2 = containers/multi-instance | Sprint 11 |
