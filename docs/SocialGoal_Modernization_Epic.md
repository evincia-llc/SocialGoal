# SocialGoal Modernization Epic -- .NET 10

**Date:** 2026-07-23
**Target:** ASP.NET Core MVC on .NET 10 LTS, EF Core, ASP.NET Core Identity, SQL Server. Single epic, no .NET Framework 4.8.x checkpoint (decision recorded 2026-07-23).
**Primary specification:** Evincia Legacy Modernization Risk Report v1.1 (`docs/Evincia-Sample-LMRR-SocialGoal.pdf`) -- phases, risk register, and sequencing law govern this plan.
**Secondary specification (gap areas only):** `docs/SocialGoal_Modernization_Risk_Report.md`, used exclusively for: (1) broken object-level authorization, (2) state-changing GET endpoints / CSRF, (3) SSRF in profile-image URL import, (4) the destructive database initializer.
**Baseline commit:** 42cfdb4

## Scope statement

This epic executes the LMRR's full four-phase program: Phase 0 (safety gate), Phase 1 (foundation retarget), Phase 2 (the two big rebuilds: System.Web removal and EF Core, plus the authentication re-platform), and Phase 3 (dependency and observability cleanup). The four security gap areas from the secondary report are not bolted on afterward: they are built into the Phase 2 rebuild as design constraints, per that report's own migration implications ("do not preserve controller/service signatures that accept a naked entity ID and assume authorization"; "all mutations become POST"; "preferably remove URL import").

Sequencing law (from the LMRR): **the safety net precedes structural change.** Sprints 1-3 earn the right to refactor; everything after runs behind that net.

Cadence assumption: 14 two-week sprints, 1-2 engineers (~7 months elapsed). Consistent with the LMRR's illustrative 6-9 months. D1 (DECIDED 2026-07-23) confirms no live users or production database, so the live-data rigor track (identity compatibility proofs on real rows, reconciliation runs, delta cutover) is out of scope for Sprints 7-8 and 14.

## Traceability: LMRR risk register disposition

| LMRR | Finding | Disposition |
|---|---|---|
| R-001 | All 7 projects target .NET Framework 4.5 | Addressed: SDK-style conversion + retarget of platform-agnostic libraries in Sprint 4 (Phase 1); System.Web/EF6-bound projects complete the move in Phase 2 (Sprints 6-11), exactly per the LMRR's recommended action. |
| R-002 | System.Web across 98 sites | Addressed: web layer re-implemented on ASP.NET Core in Sprints 9-11 -- the largest single surface, migrated as vertical slices behind the net. |
| R-003 | Web→Data reference (resolved, Low) | Residuals closed: dead `using` deleted, leaked `Page` paging type retired with the service-boundary rebuild, Identity DbContext unified (Sprint 8). Guardrail carried through Phase 2: controllers stay service-mediated; an architecture test enforces it on the new platform (Sprint 13). Composition-root wiring moves to `Program.cs`. |
| R-004 | EF6 across three projects (incl. 6.0.2-beta1) | Addressed: version unified at stable EF6 in Sprint 4 (retiring the beta pin); EF6 → EF Core migration in Sprints 6-7, gated on the Sprint 2 data-layer characterization tests. |
| R-005 | Deprecated packages incl. OWIN identity family | Addressed: OWIN identity stack replaced by ASP.NET Core Identity (Sprint 8); Web.Optimization replaced by static web assets (Sprint 12); remaining residue in Sprint 13. |
| R-006 | Authentication re-platforming | Addressed: Sprint 8, gated on the Sprint 3 auth characterization tests. Note from code read: the live stack is OWIN cookie auth + Identity 1.0; the Forms-auth material is config remnant plus dead scaffolding, removed in Sprint 1. One auth migration, Identity 1.0 → Core Identity directly. |
| R-007 | Tested, not safety-netted | Addressed in full, first: Sprints 2-3 light all three dark seams (data layer, actual auth/CSRF enforcement, DB trigger question) before any structural work. |
| R-008 | Vulnerable packages | Addressed: SBOM/SCA baseline + critical-subset remediation on the legacy build (Sprint 1/4); the Phase 2 rebuild replaces the vulnerable stack wholesale; final clean scan is the Sprint 13 exit gate. |
| R-009 | System.Data.SqlClient | Addressed: Microsoft.Data.SqlClient arrives with EF Core in Sprints 6-7 (the clean mechanical swap the LMRR describes, executed at the natural moment). |
| R-010 | MvcMailer, PagedList residue | Addressed Sprint 13: MailKit + Razor templating; built-in paging or X.PagedList. |
| R-011 | Fan-out in Web/Tests | Monitor only, per LMRR. No dedicated workstream. |
| R-012 | EF version conflict | Addressed Sprint 4 (unify), fully resolved by the EF Core migration (Sprints 6-7). |
| R-013 | AutoMapper CI pre-release | Addressed Sprint 13: stable AutoMapper with instance-based configuration, or explicit mapping where profiles are trivial. |
| R-014 | ELMAH only observability | Addressed in two steps: Sprint 1 locks the anonymous endpoint down; Sprint 13 replaces it with ILogger + Serilog/Application Insights, structured logging, metrics, health checks. |
| R-015 | Clean abstraction layers (strength) | Preserved as the migration seam. The service layer is the unit that moves; vertical slices ride it. Repository/UnitOfWork ceremony is retired in favor of EF Core-native patterns during Sprints 6-7 (the LMRR seam is the service boundary, not the repository pattern). |
| R-016 | Hardcoded test URLs | Addressed opportunistically in Sprint 13 (constants/config). |

## Secondary-report gap areas (the only items sourced from the markdown report)

| Gap | Evidence (verified against source) | Treatment |
|---|---|---|
| 1. Broken object-level authorization | `GoalController.cs:111/:128/:162/:173/:494/:541`; `GroupController.cs:154/:185/:266/:278/:322/:390/:431/:728/:840/:876`; `AccountController.cs:577` (`EditProfile` trusts posted `UserId`), `:610/:624` (accept/reject trust URL IDs) | Pinned in Sprint 3 matrix; rebuilt correctly in Sprints 9-11 via policy-based authorization and resource handlers (`CanEditGoal`, `IsGroupAdmin`, `IsRequestRecipient`); server derives owner IDs from the principal, never from posted fields. Negative tests for every mutation. |
| 2. State-changing GETs / missing CSRF | ~17 mutating GET actions (supports, follow/unfollow, accept/reject, `JoinGroup`, `DeleteMember`, `GoalStatus`, `EmailRequest` actions); `LogOff` protections commented out (`AccountController.cs:333-334`); only 7 of 32 POSTs validate a token; `AntiForgeryTokenFilterProvider` exists but is never registered | All mutations become POST in the rebuilt controllers; `AutoValidateAntiforgeryToken` applied globally to browser controllers (Sprints 9-11). HTTP-level tests enforce safe-verb semantics. |
| 3. SSRF in image URL import | `AccountController.cs:394-474` -- `WebRequest.Create` on user URL, well-formedness check only, unbounded read, bitmap decode; only outbound fetch in the codebase; the POST also lacks an antiforgery token | Flagged off in Sprint 1 (containment). Decision D3 default: feature removed in the rebuild. If retained: isolated bounded fetcher (HTTPS-only, DNS/IP validation through redirects, private/loopback/metadata deny, timeout, byte cap, signature sniff, decoded-dimension caps). Upload path hardened either way (Sprint 11). |
| 4. Destructive initializer | `GoalsSampleData : DropCreateDatabaseIfModelChanges` (`GoalsSampleData.cs:11`), registered `Global.asax.cs:16`; no EF migrations exist | Disabled in Sprint 1. Schema governed by reviewed EF Core migrations from Sprint 6 onward; deployment identities lack DDL-drop rights; restore drill in Sprint 14. |

## Codebase findings recorded during evaluation (not in either report, or sharpened by source)

* **Both custom security filters are inert.** `SocialGoalAuthorizeAttribute` and `AntiForgeryTokenFilterProvider` (filename carries a trailing space) are defined in Web.Core but never registered or applied. Actual enforcement is stock `[Authorize]` only. Phase 0 characterization targets the real enforcement; the dead filters are not ported.
* **`SearchController` has no `[Authorize]`** -- anonymous enumeration of goals, users, and groups. Decision D4.
* **Google external login is dead in practice:** `UseGoogleAuthentication()` with no credentials, on the retired Google OpenID 2.0 protocol. FB/Twitter/Microsoft providers referenced but commented out. Decision D5.
* **Email is unconfigured:** `mailSettings` commented out, `MvcMailer.BaseURL` empty, `.Send()` synchronous. Invite/welcome/reset flows silently do nothing as committed. Decision D7.
* **Config remnants:** Forms-auth section declared with `timeout="1"` while `FormsAuthenticationModule` is removed; `debug="true"`; ELMAH `requiresAuthentication=false`.
* **Bundles serve jQuery 1.7.2** even though 1.10.2 is the packaged version; loose copies of 1.2.6/1.6.2/1.7.1/`jquery-latest` plus two jQuery UI versions load together; Knockout ships unused.
* **Second DbContext per request:** `Bootstrapper.cs:42-43` constructs `UserManager(new UserStore(new SocialGoalEntities()))` outside the unit-of-work context (the concrete form of R-003's residual).
* **`ApplicationUserConfiguration` is defined but not registered** in `OnModelCreating` -- the source model cannot be assumed to describe the generated schema; the Sprint 2 schema snapshot is the schema of record.
* **`System.Drawing` in the image path** (`Bitmap`/`Graphics` in `UploadImage`) is Windows-only on modern .NET and **must** be replaced (ImageSharp or SkiaSharp) in the rebuild -- not optional under this epic's target.
* **In-proc session** via `SocialGoalSessionFacade` (`HttpContext.Current.Session`) ties the join-group/goal flow to sticky sessions; re-implemented on ASP.NET Core session/TempData in Sprint 11.

## Sprint plan

### Phase 0 -- Safety gate (Sprints 1-3)

#### Sprint 1 -- Containment and reproducible legacy build

*LMRR Phase 1 containment; Gaps #3 (temporary) and #4 (disable); R-008 baseline, R-014 partial.*

* Branch discipline: all work on feature branches; pin baseline commit 42cfdb4; protect `master`.
* Legacy CI on Windows (GitHub Actions `windows-latest`): NuGet restore, MSBuild, NUnit run. Exact toolset prerequisites captured in `docs/BUILD.md`.
* Replace `DropCreateDatabaseIfModelChanges` registration with `CreateDatabaseIfNotExists` for local dev and a null initializer everywhere else (config-switched).
* Lock ELMAH (`requiresAuthentication=true`, role restriction); `debug=false` via transform; remove the Forms-auth config remnant and dead Web.Core Forms-auth scaffolding.
* Feature-flag the image-URL import path off.
* SBOM/SCA baseline scan including vendored browser assets; record the known-vulnerable set.
* Golden-path screenshots and behavior notes for the UI (visual reference for the rebuild).

**Exit gate:** clean CI build from a fresh clone; destructive initializer unreachable; ELMAH locked; URL import off; SBOM committed.

#### Sprint 2 -- Safety net I: data layer and database truth

*R-007 (data-layer dark zone); LMRR open unknown (triggers).*

* LocalDB-backed characterization tests over `RepositoryBase`, `UnitOfWork`, `DatabaseFactory`, and each repository's query behavior -- pin current behavior, including the early-materialization semantics; do not fix yet.
* DbContext mapping smoke tests across the ~24 registered `EntityTypeConfiguration` classes; pin the actual generated model (noting the unregistered `ApplicationUserConfiguration`).
* Generate and commit a schema snapshot (DDL script + checksums) as the baseline schema of record for the EF Core migration.
* Close the trigger unknown: `SELECT * FROM sys.triggers` against the generated database and any real instance; document the answer.
* Coverage instrumentation in CI; baseline recorded (44.5% line / 23.8% branch per LMRR).

**Exit gate:** data layer no longer at 0% coverage; schema baseline committed; trigger question closed in writing.

#### Sprint 3 -- Safety net II: authorization matrix and CSRF characterization

*R-007 (auth/anti-forgery dark zone); prepares Gaps #1 and #2.*

* Test-infrastructure refresh (NUnit 3.x + adapter, current Moq) so new tests sit on a supported framework; existing ~116 tests stay green.
* Characterize actual enforcement: per-action `[Authorize]`/`[AllowAnonymous]` surface (including `SearchController`'s anonymity); proof that the two custom filters are inert.
* Build the actor × action × object matrix (owner / unrelated user / group member / group admin / request recipient / anonymous) as HTTP-level pinning tests over every mutating action. These document today's broken behavior; they become the enforcement spec for the rebuilt controllers.
* Characterize CSRF posture (7 protected POSTs, 25 unprotected, ~17 mutating GETs) and safe-verb violations.

**Exit gate:** matrix suite runs in CI; all three R-007 dark seams lit; sign-off to begin structural work.

### Phase 1 -- Foundation retarget (Sprints 4-5)

#### Sprint 4 -- SDK-style conversion and platform-agnostic retarget

*R-001, R-004 partial, R-008 critical subset, R-012.*

* Convert all 7 projects to SDK-style project files (LMRR R-001 recommended action).
* Retarget the platform-agnostic libraries (`SocialGoal.Core`, `SocialGoal.Model`, `SocialGoal.Service` as far as its dependencies allow) toward .NET 10, multi-targeting `net48;net10.0` where EF6/System.Web dependencies force a transition period. Web, Web.Core, Data, and Tests complete the move in Phase 2.
* Unify EF6 at the latest stable across Web/Data/Tests; retire the 6.0.2-beta1 pin.
* Remediate the critical subset of package vulnerabilities that can move without breaking the legacy app (e.g., Newtonsoft.Json 13.x); record breaking-bump analysis for the rest.
* Central package management + lock files for the new solution layout.

**Exit gate:** solution builds SDK-style in CI; class libraries compile for .NET 10; legacy app still runs; all suites green.

#### Sprint 5 -- Modern host foundation and migration spike

*LMRR Phase 2 entry gate; backup-report Phase 2/3 foundation, used here as engineering practice (not spec).*

* New ASP.NET Core (.NET 10) host: `Program.cs` composition root (absorbing Bootstrapper wiring), built-in DI (Autofac only if a demonstrated need survives), configuration/secrets from environment, Data Protection key persistence, error handling, health endpoint, Serilog skeleton.
* CI for the modern solution: build, tests, analyzers, formatting, SCA gate.
* **Spikes that gate the rest of the epic:**
  * EF Core mapping spike: map 3-4 representative entities (including one gnarly relationship) against the Sprint 2 schema snapshot; compare generated SQL.
  * Identity spike: stand up ASP.NET Core Identity against the existing `AspNetUsers` shape with string IDs; verify the Identity-1 password-hash format verifies under Core Identity's compatibility path (v2 format) with rehash-on-login.
  * One read-only vertical slice end to end: goal detail page rendered by the Core host from the real schema.

**Exit gate (LMRR Phase 2 entry):** architecture decision record; proven auth and data approach; one production-shaped vertical slice running. If a spike fails, revise before committing the rebuild budget.

### Phase 2 -- The two big rebuilds, behind the net (Sprints 6-11)

#### Sprints 6-7 -- EF6 to EF Core

*R-004, R-009, R-012 completion; gated on Sprint 2 tests.*

* EF Core DbContext + configurations for all ~25 entity sets, mapped to the Sprint 2 baseline schema (table/column names and string user IDs preserved; no key-type changes).
* Baseline EF Core migration matching the schema snapshot; seed data (Metrics, GoalStatus) as idempotent migration data with stable keys.
* Microsoft.Data.SqlClient (R-009 lands here).
* Retire Repository/UnitOfWork ceremony: application services move to EF Core-native query/command patterns -- projections, `AsNoTracking` reads, async end to end, explicit includes, pagination before materialization. The service-layer *interface* remains the seam; its internals modernize.
* Port Sprint 2 characterization tests to EF Core and reconcile behavior differences deliberately (query translation deltas surface here -- this is where the Phase 0 net pays off, per the LMRR).
* Unique constraints on relationship/support/invitation pairs where the pinned behavior allows (prevents duplicate-row drift in the new schema).

**Exit gate:** all data-layer characterization tests green on EF Core against SQL Server; migration baseline reviewed; no schema drift from snapshot except intentional, documented additions.

#### Sprint 8 -- Authentication re-platform

*R-005, R-006; R-003 residual; gated on Sprint 3 auth tests.*

* ASP.NET Core Identity with cookie authentication; string user IDs preserved; Identity schema updated by reviewed migration.
* Reconcile `ApplicationUser`'s duplicated email/profile fields against Core Identity's native properties; `RoleId`/group-admin booleans normalized into the policy model without losing group-scoped permissions.
* Compatibility password hasher verified (legacy hash verifies, rehash on successful login), per the Sprint 5 spike.
* Single DbContext for Identity + domain (closes R-003's residual second-context issue).
* Cookie hardening: Secure, HttpOnly, SameSite, sensible expiration.
* External logins (decision D5): dead Google OpenID wiring removed; Google OAuth 2.0 registered only if the feature is wanted.
* Account flows rebuilt: register, login, logout (POST + antiforgery), manage, external-login linking.

**Exit gate:** Sprint 3 auth characterization suite green against the Core host; password-compat test passes; logout is no longer GET-reachable.

#### Sprints 9-11 -- Web layer rebuild in vertical slices

*R-002 (the 98 System.Web sites); Gaps #1, #2, #3 built in as design constraints.*

Standing constraints for every slice: policy-based authorization with resource handlers (no naked-ID trust; owner/admin/recipient derived from the principal); all mutations POST with global `AutoValidateAntiforgeryToken`; command DTOs, not domain entities, bound from requests; matrix tests flipped from pinning to enforcement as each slice lands; Playwright journey per slice.

* **Sprint 9 -- Slices 1-3:** read-only profiles and goal browsing; goal CRUD + updates/comments/support (support actions become POST); dashboard/notifications read model.
* **Sprint 10 -- Slices 4-5:** social graph (follow request/accept/reject/unfollow -- all POST, recipient-verified); groups (membership, admin enforcement on every group mutation, focuses, group goals, group updates/comments/support, `DeleteMember` admin-gated and POST).
* **Sprint 11 -- Slices 6-7:** invitations + email tokens (expiry, purpose, recipient binding -- replacing the bare-GUID `SecurityToken`); profile-image upload rebuilt on ImageSharp/SkiaSharp with size/dimension/signature limits and safe filenames (URL import per decision D3, default removed); search (`[Authorize]` per D4); charts (jqPlot replaced by a maintained library); session-dependent join flow re-implemented on Core session/TempData.

**Exit gate (per slice):** authorization matrix green in enforcement mode; verb/CSRF tests green; Playwright journey green; slice demoed against golden-path screenshots. **Exit gate (Sprint 11):** zero System.Web references remain; legacy Web/Web.Core/Tests projects retire; full matrix green.

### Phase 3 -- Dependency, front-end, and observability cleanup (Sprints 12-14)

#### Sprint 12 -- Front-end consolidation

*R-002 tail, R-008 front-end subset.*

* Static web assets replace `System.Web.Optimization`/WebGrease; minimal modern build step only if needed.
* Single jQuery 3.7.x (or removal where `fetch` + small ES modules suffice); current validation scripts; one Bootstrap version per decision D6; delete the legacy script museum (five jQuery copies, two jQuery UI versions, Knockout, PJAX/jqModal/NiceScroll era plugins, dead themes).
* Responsive and accessibility pass as acceptance criteria (WCAG-aware, not a redesign project).

**Exit gate:** one copy of each front-end dependency, all current; no vendored known-vulnerable assets; UI parity confirmed against golden paths.

#### Sprint 13 -- Dependency residue and observability

*R-008 completion, R-010, R-013, R-014, R-016; R-003 guardrail.*

* MailKit + Razor email templating replaces MvcMailer; SMTP/provider settings from secret store; outbox-style background send for non-critical mail (decision D7 scope).
* Built-in paging or X.PagedList replaces PagedList; the leaked `Page` type is gone with the old Data project.
* Stable AutoMapper with instance configuration, or explicit mapping where profiles are trivial (R-013).
* Serilog + Application Insights (or OTel exporter per hosting decision D8): structured logs, correlation IDs, metrics, health/readiness checks. ELMAH retired (R-014).
* Architecture test enforcing controller → service mediation (R-003 guardrail, now on modern .NET).
* Test URL constants (R-016). Final SBOM/SCA: zero known-vulnerable packages.

**Exit gate:** SCA clean; observability live; architecture test in CI.

#### Sprint 14 -- Hardening, cutover, close-out

*LMRR Phase 3 completion; secondary-report cutover discipline (engineering practice).*

* Security headers (HSTS, CSP, X-Content-Type-Options, frame protection, referrer policy); HTTPS-only, secure cookie audit.
* Restore drill: backup, point-in-time recovery, migration rollback rehearsal against a production-shaped copy.
* If any live deployment exists (D1): reconciliation checks (row counts, relationship counts, login samples), canary window, time-bounded rollback path.
* Legacy projects, packages, and CI lanes deleted; `docs/` updated; readiness re-score against the LMRR trajectory (target: Green zone, ~88 per the LMRR projection).
* Epic close-out review against this document; deferred/out-of-scope register handed to the backlog.

**Exit gate:** legacy code removed from the build; all suites green; close-out review complete.

## AI use-case sequencing (from the LMRR)

* **Safe during Sprints 1-5:** read-only analysis, AI-assisted characterization-test generation against the dark zones, AI-assisted csproj conversion proposals.
* **Safe during Sprints 6-11:** sandboxed AI code transformation for the System.Web rewrite, EF6→EF Core migration, and Identity scaffolding -- behind the Phase 0 net, in non-production environments.
* **Safe after Sprint 13:** AI in the live pipeline (assisted review, observability/anomaly tooling on the new logging stack, dependency maintenance).

## Open decisions

| # | Decision | Default recommendation |
|---|---|---|
| D1 | Is there any live deployment/database/user population? | **DECIDED 2026-07-23: none** (see `ai-context/decisions.md`). Live-data rigor track dropped from Sprints 7-8 and 14. |
| D2 | Hosting target (App Service, containers, on-prem) | Affects D8, image storage (D9), and Data Protection key persistence. Needed by Sprint 5. |
| D3 | Image URL import: remove or harden | Remove. The bounded-fetch service is significant scope for a marginal feature. |
| D4 | `SearchController` anonymous or authorized | Add `[Authorize]`; the rest of the app is authenticated-only. |
| D5 | External logins | Remove dead Google OpenID wiring; add Google OAuth 2.0 in Sprint 8 only if the feature is wanted (new registration + redirect URLs either way). |
| D6 | Bootstrap 3.4.1 (parity, minimal change) vs Bootstrap 5 (modern, more view work) | 3.4.1 for parity within this epic; Bootstrap 5 as a follow-on UI project unless stakeholders want it now (adds effort per the secondary report's 30-75% redesign range). |
| D7 | Must email flows actually send? | Configure MailKit against a real provider only if invites/reset are required; otherwise no-op mailer with logging, flows kept testable. |
| D8 | Observability backend: Application Insights vs OTel + other APM | Follows D2. |
| D9 | Profile images: local disk vs object storage | Object storage if D2 is containers/multi-instance; local disk acceptable for single-instance Windows hosting. |

## Out-of-scope register

* SPA/React front end, public Web API, mobile clients (the README's planned API was never implemented; no contract exists to preserve). New product surface, not modernization.
* Microservices decomposition -- modular monolith per both reports.
* Product redesign/UX overhaul beyond parity + accessibility criteria (see D6).
* MFA, email confirmation, account-recovery policy beyond what exists -- flagged by the secondary report outside the four sanctioned gap areas; belongs to a product-security backlog, not this epic.

## Out-of-spec observations (recorded, scheduled only where the rebuild absorbs them naturally)

* Synchronous data/service layers → absorbed by Sprints 6-7 (async end to end).
* `DateTime.Now` vs UTC; `double` for phone/ZIP fields → type/UTC corrections land with EF Core entity definitions (Sprints 6-7), flagged as intentional schema deltas.
* Token lifecycle weaknesses (`SecurityToken` bare GUIDs) → absorbed by Sprint 11 (invitations slice).
* Duplicate-row exposure on relationship tables → unique constraints in Sprints 6-7.
