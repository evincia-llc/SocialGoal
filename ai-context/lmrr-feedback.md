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

### Missed candidates (Category 11 scope caveat applies; candidate engine signals)

- **Destructive initializer · MISSED-CANDIDATE:** `Database.SetInitializer` +
  `DropCreateDatabaseIfModelChanges` subclass (`GoalsSampleData.cs:11`,
  `Global.asax.cs:16`). Statically detectable -- strong engine-predicate
  candidate.
- **State-changing GET actions · MISSED-CANDIDATE:** ~17 mutating actions with
  no `[HttpPost]`. Detectable via attribute analysis on controller actions that
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
