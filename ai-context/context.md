# Project context snapshot

Last verified: 2026-07-23 (full codebase survey + both reports evaluated).

## Goal / vision: this epic is an Evincia POC

The modernization is real, but the purpose is meta: determine whether Evincia
(evincia.co) gains meaningful, valuable knowledge about modernizing a target
codebase (public teardown or client engagement) by attempting the modernization
itself. Three feedback products, all first-class deliverables alongside the code:

1. `journal.md` -- the honest friction record (feeds the end-of-epic report).
2. `lmrr-feedback.md` -- per-finding validation of the LMRR: confirmations,
   corrections, false positives, misses, and candidate engine predicates.
3. Effort actuals vs the LMRR's illustrative estimates (engagement scoping data).

False positives/negatives discovered by doing the work correct the LMRR itself.
Engine-predicate candidates route to the evincia-ai-workflows backlog; engine
data never lands in this repo.

## What this is

Modernization of SocialGoal (public MarlabsInc ASP.NET MVC 5 / EF6 / .NET 4.5
reference app, last commit June 2014, baseline commit `42cfdb4`) to ASP.NET Core
MVC on .NET 10, EF Core, ASP.NET Core Identity. Single epic, 14 two-week sprints
in 4 phases -- see `docs/SocialGoal_Modernization_Epic.md`. LMRR readiness score
at baseline: 44/100 (Red); target after Phase 3: ~88 (Green).

Assumed: no live deployment, users, or production database (decision D1 -- confirm
before Sprint 5). The epic's rigor escalates if that assumption breaks.

## Solution shape (7 projects, all .NET Framework 4.5)

Web (MVC 5.0) -> Service (~28 services) -> Data (EF6 repos/UnitOfWork,
`SocialGoalEntities : IdentityDbContext<ApplicationUser>`) -> SQL Server.
Plus: Model (~30 entities), Core (one file), Web.Core (filters/helpers),
Tests (NUnit 2.6.3, ~116 controller tests). Layering is clean and is the
migration seam (LMRR R-015); Web->Data reference is composition-root wiring
(R-003, resolved Low).

## Key facts that shape the work

- **Dark seams (R-007):** data layer 0% coverage, auth/anti-forgery filters 0%,
  DB triggers unknown. Phase 0 (Sprints 1-3) lights these before anything moves.
- **Security gaps (secondary report, the four sanctioned areas):**
  - BOLA: goal/group/profile mutations trust caller-supplied IDs
    (`GoalController.cs:111-181,494,541`; `GroupController.cs:154-876` mutations;
    `AccountController.cs:577,610,624`).
  - ~17 state-changing GETs; `LogOff` protections commented out
    (`AccountController.cs:333-334`); only 7 of 32 POSTs validate antiforgery.
  - SSRF: `GetImageFromUrl` (`AccountController.cs:394-474`), the only outbound
    fetch in the codebase.
  - `GoalsSampleData : DropCreateDatabaseIfModelChanges`, registered
    `Global.asax.cs:16`. No EF migrations exist anywhere.
- **Inert security code:** `SocialGoalAuthorizeAttribute` and
  `AntiForgeryTokenFilterProvider` (filename has a trailing space) exist in
  Web.Core but are never registered. Real enforcement is stock `[Authorize]` only.
- **`SearchController` has no `[Authorize]`** (anonymous enumeration) -- D4.
- **Auth stack:** OWIN 2.0 cookie auth + Identity 1.0. Google external login is
  dead (no creds, retired OpenID 2.0 protocol) -- D5. Forms-auth config is remnant.
- **Second DbContext per request** for Identity (`Bootstrapper.cs:42-43`) --
  unified in Sprint 8.
- **Email unconfigured** (mailSettings commented out; MvcMailer BaseURL empty) -- D7.
- **Version skew:** EF 6.0.2-beta1 (Web) / 6.0.1 (Data) / 6.0.0 (Tests);
  AutoMapper 3.1.1-ci1000; Newtonsoft 5.0.6; bundles serve jQuery 1.7.2 while
  1.10.2 is packaged; five loose jQuery copies + two jQuery UI versions.
- **`System.Drawing`** in the image path must be replaced (Windows-only on
  modern .NET); in-proc session ties the join flow to sticky sessions.
- **ELMAH is anonymous as committed** (`elmah.mvc.requiresAuthentication=false`)
  and `debug="true"` -- locked down in Sprint 1.
- **`ApplicationUserConfiguration` is defined but not registered** in
  `OnModelCreating` -- the Sprint 2 schema snapshot, not the source model, is the
  schema of record.

## Key file paths

- DI/composition: `source/SocialGoal/App_Start/Bootstrapper.cs`
- Auth: `source/SocialGoal/App_Start/Startup.Auth.cs`
- DbContext: `source/SocialGoal.Data/SocialGoalEntities.cs`
- Initializer: `source/SocialGoal.Data/GoalsSampleData.cs` + `source/SocialGoal/Global.asax.cs`
- Config: `source/SocialGoal/Web.config`
- SSRF/upload: `source/SocialGoal/Controllers/AccountController.cs:394-474`
