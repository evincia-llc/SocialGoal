# LMRR feedback register

The POC's core instrument (see `context.md`, Goal/vision): a per-finding ledger
of what actually doing the modernization proves about the Evincia LMRR
(`docs/Evincia-Sample-LMRR-SocialGoal.pdf`, v1.1). Feeds the LMRR revision and
the end-of-epic report.

**Statuses:** `CONFIRMED` (finding held up in practice) · `CORRECTION-CANDIDATE`
(finding real, description/severity needs adjustment) · `FALSE-POSITIVE-CANDIDATE`
· `MISSED-CANDIDATE` (real issue the LMRR did not carry) · `METHODOLOGY-NOTE`.
Suffix `(pending)` marks entries whose remaining evidence must come from the
implementation phase; entries fully verifiable by static source inspection
(e.g., R-016) may be confirmed outright.

**Rules:** every entry cites evidence (file:line, commit, journal entry, or
sprint result). Candidate engine predicates route to the evincia-ai-workflows
backlog as pointers -- no engine data in this repo. Scope fairness: the four
secondary-report gap areas fall under Category 11 (business-logic review), which
this LMRR explicitly did not perform; MISSED entries carry that caveat and are
methodology input, not scoring errors.

## Register

### Confirmations (from the pre-Sprint 1 evaluation; implementation will retest)

- **R-003 · CONFIRMED (pending):** independent code survey reproduced the v1.1
  ruling exactly -- composition-root wiring, one dead `using`
  (AccountController), one leaked `Page` type, second Identity DbContext
  (`Bootstrapper.cs:42-43`).
- **R-016 · CONFIRMED:** hardcoded test URLs found as described.
- **R-007 · CONFIRMED (minor count nuance, pending):** survey counted ~116
  `[Test]`-family attributes vs the LMRR's 113 passing tests; dark-seam claims
  (data layer, auth/anti-forgery filters at 0%) verified structurally -- the
  filters are in fact never registered at all (see MISSED below).
  Sprint 1 update: first full test run executed exactly **113 tests, 113
  passing** (NUnit 2.6.4 console, 2026-07-23) -- the LMRR's count was the
  precise one; the attribute survey over-counted by ~3.
- **R-001/R-002/R-004/R-005/R-012/R-013 · CONFIRMED (pending):** all version
  pins, the 6.0.2-beta1 EF pin, AutoMapper `3.1.1-ci1000`, and framework
  targets verified against packages.config/csproj files.

### Correction candidates

- **R-006 · CORRECTION-CANDIDATE (pending):** titled "Forms Authentication
  Needs Re-Platforming," but the live stack is OWIN cookie auth + ASP.NET
  Identity 1.0; Forms auth exists only as a Web.config remnant (`timeout="1"`,
  module removed) and fully commented-out scaffolding in Web.Core. Remediation
  unchanged; description imprecise. Sprint 8 will confirm.
  Sprint 1 nuance: the scaffolding is *not* uniformly dead. Tests build
  principals through `DefaultFormsAuthentication`/`SocialGoalUser`,
  `Bootstrapper.cs:38` anchors DI scanning on the type, and
  `_UpdateView.cshtml:154` casts `User.Identity` to `SocialGoalUser` -- a cast
  that throws under the live OWIN stack (`ClaimsIdentity`), i.e. a latent
  runtime defect. "Config remnant + dead scaffolding" should read "config
  remnant + inert-for-auth types with live references, one of them broken."
  Candidate engine signal: casts of `IPrincipal.Identity`/`User.Identity` to a
  concrete type not produced by the configured auth stack.

### Sprint 2 characterization findings (data layer, 2026-07-24)

- **R-007 · dark seam lit:** data layer now has 28 executing characterization
  tests (LocalDB), pinning repository/UoW/mapping behavior; 0% coverage claim
  ends here. Two hazards the LMRR's structural read implied are now *proven by
  failing-write test*: (a) `RepositoryBase.Update` attaches + marks the whole
  entity Modified -- a stale detached copy silently nulls unset columns;
  (b) repositories and UnitOfWork on *different* `DatabaseFactory` instances
  lose writes silently (only DI's single shared factory makes the app work).
- **Dead-config candidates (extends the "defined but never registered"
  missed-candidate below):** `ApplicationUserConfiguration` unregistered (its
  intent even carries a `FirstName.HasMaxLength(1)` bug); `GoalUpdateConfiguration`
  orphaned twice over (unregistered AND no DbSet -- entity absent from the
  model). Engine predicate: EntityTypeConfiguration subclasses vs
  OnModelCreating registrations vs DbSet presence -- three-way diff, all
  statically detectable.
- **Model facts for the migration:** 30 entity sets, all dbo; EF pluralization
  quirks pinned (`Focus->Foci`, `GoalStatus->GoalStatus`); `RegistrationToken`
  enters the model via registration despite having no DbSet.

### Sprint 3 characterization findings (authz/CSRF surface, 2026-07-24)

- **State-changing GETs · count corrected:** the secondary report's "~17"
  mutating GETs is, on a full reflection census, **23** -- Goal 5, Group 10
  (incl. `SaveUpdate` whose `[HttpPost]` is commented out), Account 6 (incl.
  `LogOff`), EmailRequest 2. Pinned by `EnforcementDefectPinTests` over the
  149-action surface table. Sharpens the MISSED-candidate below with an exact,
  test-enforced number.
- **CSRF split · confirmed exactly:** 32 POSTs, exactly 7 with
  `[ValidateAntiForgeryToken]` (all `AccountController`), 25 without. No global
  antiforgery filter; the would-be global provider is dead (below).
- **Inert security filters · CONFIRMED (was pending):** the "defined but never
  registered" missed-candidate is now test-proven, not just inferred --
  `FilterConfig` registers only `HandleErrorAttribute`;
  `SocialGoalAuthorizeAttribute` and `AntiForgeryTokenFilterProvider` are
  referenced by no controller/action/registration (`InertFilterCharacterization
  Tests`). `Bootstrapper.cs:45` `RegisterFilterProvider()` is Autofac's provider,
  not the custom one -- worth noting as a false-comfort trap for a human
  reviewer (a declared "authorize filter" that does nothing).
- **`GroupUser.Admin` gates nothing · MISSED-CANDIDATE (new):** the admin flag
  is persisted and read only for UI rendering (`GroupController.cs:93`), never
  as an authorization check on any group mutation. Group admin, member, and
  unrelated user are the same authorization principal on edit/delete/member/
  focus/goal/accept/reject. Candidate engine signal: a persisted role/permission
  field that is written and read-for-display but never used in an authorization
  branch. (Category 11 scope caveat applies; strengthens the BOLA candidate.)
- **`Account.EditProfile` cross-user write · new BOLA instance:** binds the
  target from `editedProfile.UserId` (posted model), so any authenticated user
  edits any other user's profile/name/email. Not in the original gap
  enumeration's named examples; found by the action census.
- **BOLA now proven behaviorally, not just inferred:** 27 controller-invocation
  tests over LocalDB (`source/SocialGoal.Tests/Authorization/*MatrixTests.cs`)
  demonstrate real persisted mutations by an unrelated authenticated user across
  Goal/Group/Account, and the Admin-flag-never-gates collapse (non-admin member
  AND unrelated both mutate a group identically to the admin). This upgrades the
  BOLA missed-candidate from static-inferred to executable evidence. Gap #1 is
  no longer a checklist assertion -- it is a red test the Phase 2 rebuild turns
  green as a 403.
- **Two "accidental gates" · METHODOLOGY-NOTE:** `Account.Unfollow` (unrelated
  caller -> `ArgumentNullException` on `DbSet.Remove(null)`) and
  `Group.CreateGoal` (non-member -> NRE on null `GroupUser`) reject an
  unauthorized call only because the code crashes, not because it checks. A
  static "is there an authorization check" scan would see no check (correct); a
  dynamic "does the unauthorized call fail" scan would see it fail (misleading).
  Engine/reviewer signal: absent-authz masked by a null-deref is still absent
  authz -- do not credit the crash as a control.
- **`Group.JoinGroup` open-join · MISSED-CANDIDATE (new):** self-scoped (joiner
  is the principal, no forged id) but ungated -- any authenticated user
  self-joins any group with no invitation/approval. A distinct shape from
  caller-supplied-id BOLA; an authorization-absence detector keyed only on
  "caller-supplied entity id" would miss it. Signal: a create of a
  membership/relationship row with no approval/invitation precondition.
- **Invitation token is a bearer secret · MISSED-CANDIDATE (new):** the
  `SecurityToken` guarding `EmailRequest.AddGroupUser`/`AddSupportToGoal` is not
  bound to the invited principal -- any authenticated token-holder joins/supports
  as themselves. Signal: a capability token consumed without checking it was
  issued to the current principal.

### Sprint 4 findings (foundation retarget, 2026-07-24)

- **R-004/R-012 · CONFIRMED, plus an uncalled-out consequence:** the version
  skew was real and unification was clean mechanically -- but unifying
  EF 6.0.x -> 6.5.2 with zero model/code change flipped `AspNetUsers.UserName`
  from `NULL` to `NOT NULL` in generated DDL (caught by the Sprint 2 drift
  test; accepted under D14, journal 2026-07-24). The LMRR treats version skew
  as a dependency-hygiene risk; the evidence shows ORM version drift is also a
  *schema-change vector*. Candidate engine signal: EF6 version spread across
  projects scores higher when a Code First model exists (generated DDL is
  version-dependent), independent of the packages' own advisories.
- **R-001 · CORRECTION-CANDIDATE (layering nuance):** the retarget confirmed
  the framework pins, but the LMRR's "platform-agnostic libraries" framing
  overstates the seam: `SocialGoal.Model` is not platform-agnostic (its
  `ApplicationUser : IdentityUser` welds the domain model to ASP.NET Identity
  1.0/EF6, and `ProfilePic.cs` pulls `System.Web` into the entity layer), and
  `SocialGoal.Service` carries a service->web-layer reference to
  `SocialGoal.Web.Core` -- vestigial on inspection (a single dead
  `using SocialGoal.Web.Core.Models;` in `GroupUpdateServices.cs:6`, no type
  usage), but it prices as a real edge until proven dead, which is itself the
  point: the dependency graph overstates coupling and only inspection shows it.
  Of the three nominally portable libraries only
  `SocialGoal.Core` (one file) reaches net10.0 in Phase 1. Candidate engine
  signals: auth-framework base classes in the entity/model assembly;
  System.Web references outside web-named projects; service->web project
  edges. All three are statically detectable and directly price the migration.

### Missed candidates (Category 11 scope caveat applies; candidate engine signals)

- **Destructive initializer · MISSED-CANDIDATE:** `Database.SetInitializer` +
  `DropCreateDatabaseIfModelChanges` subclass (`GoalsSampleData.cs:11`,
  `Global.asax.cs:16`). Statically detectable -- strong engine-predicate
  candidate.
- **State-changing GET actions · MISSED-CANDIDATE:** 23 mutating actions with
  no `[HttpPost]` (census-corrected from the report's ~17; see Sprint 3
  findings). Detectable via attribute analysis on controller actions that
  call known-mutating service methods; even the attribute-only heuristic has
  signal.
- **Defined-but-never-registered security filters · MISSED-CANDIDATE:**
  `AntiForgeryTokenFilterProvider` and `SocialGoalAuthorizeAttribute` exist but
  are wired nowhere. "Security type declared, zero registrations/usages" is
  statically detectable.
- **`[Authorize]`-less controller in an authenticated app · MISSED-CANDIDATE:**
  `SearchController` anonymous while every sibling is `[Authorize]`. Outlier
  detection over controller attributes.
- **User-input URL to `WebRequest.Create` (SSRF) · MISSED-CANDIDATE:**
  `AccountController.cs:394-474`. Taint-style detection is harder; a
  "WebRequest/HttpClient fed from action parameter/model" heuristic may be
  worth piloting.
- **Broken object-level authorization · MISSED-CANDIDATE (hardest):** naked-ID
  mutations without ownership checks. Likely stays senior-architect work;
  register the pattern in the Category 11 checklist rather than the engine.

### Methodology notes

- **Trigger unknown closable by construction:** with no live DB and Code First
  (D1), `sys.triggers` cannot carry hidden logic predating the project. The
  LMRR template could conditionally close this unknown when an engagement
  confirms no live database. Sprint 2 documents the check anyway.

## Effort actuals vs LMRR illustrative estimates

Fill at each sprint-gate. LMRR baselines: Phase 0 "a few weeks, 1 eng"; Phase 1
"1-2 months, 1-2 eng"; Phase 2 "3-5 months, 1-2 eng"; Phase 3 "1-2 months, 1 eng".

| Sprint | Planned (epic) | Actual | Notes |
|---|---|---|---|
| Foundation (pre-S1) | -- | 1 day (2026-07-23) | Evaluation, epic, governance, tooling |
| Sprint 1 | 2 weeks | ~half a working day (2026-07-23) | All deliverables: proven build + CI, initializer switch, ELMAH lock, SSRF flag, SBOM/SCA + security lane, golden paths. AI-driven pace; multi-user golden paths deferred to S3 |
| Sprint 2 | 2 weeks | ~1 working day (2026-07-24) | Harness + 31 data tests (142->144 suite), schema baseline + drift tests, trigger closed, OpenCover in CI (incl. profiler-flake fix), PR loop. Includes crashed-session recovery |
| Sprint 3 | 2 weeks | ~1 working day (2026-07-24) | NUnit 3/Moq 4.20/net48 refresh, 149-action surface census, 27-test behavioral matrix (suite 187), matrix doc = Phase 2 enforcement spec, D11. Phase 0 total: ~2.5 days vs LMRR "a few weeks" |
