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

### D11 -- Authz/CSRF matrix test level: controller-invocation + reflection, not out-of-proc HTTP

- **Status:** DECIDED · 2026-07-24 · Owner: Claude · **Ratified by operator
  2026-07-24** (Sprint 4 start)
- **Context:** The epic's Sprint 3 text calls for "HTTP-level pinning tests"
  over every mutating action. MVC 5 on System.Web has no in-process HTTP test
  host: `Microsoft.Owin.Testing.TestServer` cannot host System.Web-routed MVC,
  and ASP.NET Core's `TestServer` does not exist for Framework MVC. The only
  true-HTTP option is out-of-process IIS Express automation (launch, poll,
  cookie/antiforgery scraping per actor), which is slow and flake-prone in CI
  for a matrix of ~30 mutating actions x 6 actor classes.
- **Decision:** Pin the matrix in two layers instead:
  1. **Declarative surface** -- reflection tests over every controller action:
     `[Authorize]`/`[AllowAnonymous]`, HTTP verb attributes,
     `[ValidateAntiForgeryToken]`, plus proof the two custom Web.Core filters
     are never registered. Stock attribute semantics are trusted, so attribute
     presence pins the HTTP-facing enforcement exactly.
  2. **Behavioral matrix** -- controller-invocation tests against the real
     service/repository/LocalDB stack (Sprint 2 harness), actor x action x
     object, pinning today's outcomes including the BOLA defects.
  A thin IIS Express HTTP probe layer (a handful of sample requests validating
  that the reflected surface matches real HTTP behavior) is attempted only if
  it proves stable; dropped without replacement if flaky.
- **Consequence:** Same seams lit, same Phase 2 enforcement spec, CI stays
  reliable. The Sprint 9-11 rebuild gets true HTTP-level tests naturally
  (ASP.NET Core TestServer), where the matrix flips from pinning to
  enforcement. Friction journaled 2026-07-24; LMRR feedback candidate (the
  "HTTP-level pinning" remediation guidance is not directly implementable on
  System.Web hosts).

### D12 -- Multi-user screenshot golden-paths deferred to the Sprint 10 slice

- **Status:** DECIDED · 2026-07-24 · Owner: Jerry (confirmed at Sprint 4 start)
- **Context:** Sprint 1 captured 13 single-user golden-path screenshots. The
  multi-user flows (group membership, feed interactions across actors) were not
  screenshot-captured in Phase 0; their behavior is pinned instead by the
  Sprint 3 behavioral matrix (27 controller-invocation tests over LocalDB).
- **Decision:** Defer multi-user *screenshot* golden-path capture to the
  Sprint 10 vertical slice that migrates those flows, where before/after
  screenshots are taken as part of slice parity evidence.
- **Consequence:** Phase 0 exit stands on the test matrix, not screenshots, for
  multi-user behavior. The Sprint 10 slice checklist gains a
  capture-before-migrating step.

### D13 -- Sprint 4 SDK-conversion mechanics

- **Status:** DECIDED · 2026-07-24 · Owner: Claude (session decision; operator
  may overturn at Sprint 4 PR review)
- **Context:** The epic mandates SDK-style conversion of all 7 projects but not
  the mechanics. Constraints discovered in inventory: `Microsoft.NET.Sdk` has no
  System.Web web-application support; EF6 latest stable (6.5.2) requires
  >= net462 while six projects target net45; the Web project depends on
  `Microsoft.WebApplication.targets` (currently shimmed in CI); packages.config
  HintPaths are inconsistent (Model references Identity assemblies with no
  packages.config at all).
- **Decision:**
  1. **Whole solution retargets net45 -> net48** (Tests already there). 4.5 is
     compile-time only -- the app already runs on the 4.8 in-place CLR -- and
     net48 unlocks EF 6.5.2 and retires the pinned net45 reference-assembly shim.
  2. **Web project converts via `MSBuild.SDK.SystemWeb` (pinned 4.0.107 in the
     csproj `Sdk` attribute)** -- the community SDK for SDK-style System.Web
     apps (desktop MSBuild only, which CI already uses). The other six use
     `Microsoft.NET.Sdk`.
  3. **PackageReference everywhere + central package management**
     (`Directory.Packages.props`, transitive pinning on so EF unifies at 6.5.2
     even where transitive) **+ committed lock files**, restored in locked mode
     in CI.
  4. **Content-only NuGet packages are dropped** (bootstrap, jQuery,
     jQuery.Validation, Microsoft.jQuery.Unobtrusive.Validation, Modernizr,
     Respond, T4Scaffolding.Core): under PackageReference they deliver nothing
     (the app serves the vendored copies in `Scripts/`/`Content/`, tracked by
     the retire.js baseline until Sprint 12). This shrinks the NuGet audit
     baseline -- allowed without decision, noted here for transparency.
  5. **Hand-written `AssemblyInfo.cs` files are deleted** in favor of
     SDK-generated attributes.
- **Consequence:** BUILD.md and legacy-ci.yml lose the net45 targeting-pack and
  VSToolsPath shims; test/coverage paths move to `bin\Release\net48\`. The
  legacy app must be re-proven running (exit-gate criterion). MVC stays 5.0.0
  and Identity stays 1.0.0 (schema-coupled; Phase 2), recorded in the Sprint 4
  breaking-bump analysis.

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
