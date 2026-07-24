# Modernization journal -- problems and roadblocks

Purpose: the raw material for an end-of-epic report on the problems discovered and
issues raised while modernizing the legacy SocialGoal codebase. Every problem,
surprise, blocker, or roadblock gets an entry **when it happens**, not
reconstructed later. Small annoyances count; the report's value is the honest
texture of what legacy modernization actually costs.

Not a duplicate of `tasks.md` (state) or `decisions.md` (choices): this file
records friction. If a problem forces a decision, log it here and link the
decision ID.

## Entry template

```
### YYYY-MM-DD · Sprint N · short title
- **Problem:** what was hit, concretely
- **Where:** file/task/tool
- **Impact:** time lost, scope effect, severity
- **Resolution:** fixed / worked around / OPEN (link D-id if escalated)
- **Report note:** what kind of problem this is (legacy defect, spec gap,
  tooling gap, dependency surprise, hidden behavior) -- the report will group
  by these
```

## Log (newest first)

### 2026-07-24 · Sprint 6 · In-suite migration drift check silently wrong without designTime finalization

- **Problem:** the CI-friendly equivalent of `dotnet ef migrations
  has-pending-model-changes` (comparing the migration snapshot model to the
  current model via `IMigrationsModelDiffer`) reported ~20 spurious
  `AlterColumn` operations -- one per identity PK -- when the snapshot model was
  initialized as a *runtime* model. Before/after were textually identical; the
  disagreement was invisible provider annotations. Only a cross-check against
  the real ef tool (which said "no changes") exposed the false positive; the
  naive test would have driven a "fix" for non-existent drift.
- **Where:** `src/SocialGoal.Web.Tests/Data/MigrationModelDriftTests.cs`
  (work unit 2); resolution is `IModelRuntimeInitializer.Initialize(...,
  designTime: true)` to match the diff's right-hand side, commented in place.
  Uses EF-internal APIs (EF1001) because no public equivalent exists.
- **Impact:** roughly an hour of implementor diagnosis; caught pre-commit.
- **Resolution:** fixed; the check was then mutation-tested in both directions
  (model change -> named-column failure + tool agreement).
- **Report note:** tooling gap -- EF Core exposes drift checking only as a CLI
  command, so putting the same guarantee in a test suite requires internal
  APIs and a non-obvious finalization step. Anyone modernizing with
  "migrations reviewed, drift gated in CI" discipline will hit this.

### 2026-07-24 · Sprint 6 · Migrations toolchain friction (analyzer gate, manifest location, script --no-build)

- **Problem:** three small snags standing up the EF migrations toolchain:
  (1) scaffolder-generated migration code fails the solution's analyzer gate
  (`CA1861` under `TreatWarningsAsErrors` + latest-recommended) -- and
  hand-editing generated code is a trap because the next scaffold reverts it;
  (2) `dotnet new tool-manifest` wrote `dotnet-tools.json` to the repo root
  rather than the conventional `.config/`; (3) `dotnet ef migrations script
  --no-build` right after `migrations add` silently emitted only the history
  table (add builds *before* writing the new files -- rebuild first).
- **Where:** `src/SocialGoal.Web/Data/Migrations/` + `.config/dotnet-tools.json`
  (dotnet-ef 10.0.10 pinned as a local tool).
- **Impact:** ~15 minutes combined; (1) recurs every sprint that scaffolds a
  migration.
- **Resolution:** (1) Migrations-folder-scoped `.editorconfig` suppressing
  exactly CA1861, with a rule that additions must name the migration that
  forced them (reviewed and accepted as build policy); (2) manifest moved to
  `.config/`, tool restore verified; (3) rebuild-then-script noted here.
- **Report note:** tooling gap (framework scaffolder output vs strict analyzer
  gates) -- a friction class every analyzer-as-errors modernization will meet
  the day it adopts EF migrations.

### 2026-07-24 · Sprint 6 · EF Core FK-index convention emits 26 indexes, not one per FK

- **Problem:** the expected "one EF Core index per FK column" delta over the
  baseline came out at 26 for 28 FKs. EF Core's convention skips a column that
  already leads an existing index: `AspNetUserLogins.UserId` and
  `AspNetUserRoles.UserId` lead their tables' composite PKs, so no extra index
  is created; `AspNetUserRoles.RoleId` (second PK column) still gets one.
- **Where:** `src/SocialGoal.Web.Tests/Data/SchemaParityTests.cs` index-delta
  pin, work unit 1 of the EF6 -> EF Core port.
- **Impact:** minutes (implementor caught it while pinning the delta). Recorded
  because the baseline migration (work unit 2) must reproduce the same 26 -- if
  the migration's generated DDL disagrees with the model's EnsureCreated, this
  convention is the first suspect.
- **Resolution:** fixed -- the delta is pinned as an exact sorted 26-entry list.
- **Report note:** dependency surprise (framework convention subtlety), the
  benign kind: parity tests turned it from a silent divergence into a counted,
  documented addition. Also worth reporting: the full 30-table port itself
  landed green on the first parity run with zero mapping iterations -- the
  Sprint 5 spike (TPH nullability, shadow FKs, datetime, EF6 constraint names)
  had already absorbed the discovery cost. Spike-first sequencing pays.

### 2026-07-24 · Phase 1/2 boundary · Which model implemented Sprints 4-5 is unverifiable (floating alias + session vintage)

- **Problem:** two sessions gave contradictory, unprovable answers to "what Opus
  version ran the implementor?" The implementor was pinned to the `opus` tier
  alias, which floats to the running Claude Code build's current Opus. Opus 5
  shipped mid-epic. A monitor session opened the night before (older build)
  probed the alias and got `claude-opus-4-8`; the Sprint 5 session (launched
  after, newer build) reported `claude-opus-5` but by its own account had no hard
  evidence -- transcripts don't record a subagent's resolved model per run.
- **Where:** `.claude/agents/implementor.md` (`model: opus`); cross-session.
- **Impact:** no delivery impact, but a real auditability gap for a POC whose
  headline is "which AI did this, how fast." The concrete version that built
  Sprints 4-5 cannot be established after the fact.
- **Resolution:** fixed forward -- pinned to explicit `claude-opus-5` (D16);
  Sprints 4-5 stay attributed as "Opus, unverified, most likely 5" in the effort
  record.
- **Report note:** tooling gap / reproducibility hazard. A long-lived agent
  session can silently keep running an OLDER model than a freshly-launched one,
  and floating model aliases make "who did the work" unanswerable in hindsight.
  The mitigation is to pin explicit model IDs, not aliases, in any auditable
  AI-delivery pipeline -- a transferable methodology finding, not SocialGoal-
  specific.

### 2026-07-24 · Sprint 5 · GitHub PR-creation outage mid pr-flow

- **Problem:** `gh pr create` failed with GraphQL 500s (3 attempts), and the
  REST fallback (`POST /repos/.../pulls`) returned HTTP 500 with an empty
  body even for a minimal payload -- PR creation itself was down server-side
  while pushes, Actions runs, and reads all worked.
- **Where:** pr-flow step 8, Sprint 5 PR.
- **Impact:** delayed PR open; retry loop.
- **Resolution:** background retry loop (75s interval) succeeded on attempt 3
  (~3 min outage window) -- PR #11. The `gh pr edit --add-reviewer Copilot`
  GraphQL path also failed ("could not resolve user"); the REST
  `requested_reviewers` endpoint with `copilot-pull-request-reviewer[bot]`
  worked.
- **Report note:** tooling gap (external service) -- the PR step is the one
  pipeline stage with no local fallback; worth remembering that "raise the PR"
  can fail independently of everything CI proves.

### 2026-07-24 · Sprint 5 · Legacy publish is code-only under MSBuild.SDK.SystemWeb (Sprint 4 regression, found by the transform proof)

- **Problem:** the one-time Web.Release.config publish proof (Sprint 4
  security-reviewer question) PASSED on both transform criteria
  (DatabaseInitializer -> None; debug attribute stripped) but exposed two
  surprises. (1) `/p:WebPublishMethod=FileSystem` alone is silently ignored --
  `DeployOnBuild=true` defaults to a WebDeploy package and reports success
  while the requested publish folder is never created; `/p:DeployTarget=
  WebPublish` is mandatory. (2) MSBuild.SDK.SystemWeb 4.0.107's default
  Content globs cover Web.configs and WebForms extensions but **no `.cshtml`
  and no Scripts/Content/fonts/Images**, so the published tree is code-only
  (bin + Global.asax + Web.config) -- not runnable as an MVC app. The publish
  path regressed at the Sprint 4 SDK conversion (D13) and nothing noticed
  because no path publishes the legacy app.
- **Where:** source\SocialGoal\SocialGoal.Web.csproj publish
  (delegated proof run, 2026-07-24; output verified in scratch).
- **Impact:** none operationally (D1: no deployment exists; the legacy app
  runs from the source folder under IIS Express, which is how every gate
  smoke-proof runs it). But "we could still publish the legacy app" would be
  false comfort if anyone relied on it as a rollback path.
- **Resolution:** documented, deliberately NOT fixed -- the legacy publish
  path has no consumer for the remainder of the epic (D2 deploys the modern
  host; legacy Web retires Sprint 11). Advisor call, flagged for operator at
  the Sprint 5 PR. If a legacy publish is ever actually needed, the fix is
  explicit Content globs in SocialGoal.Web.csproj plus a re-proof.
- **Report note:** dependency surprise / hidden behavior -- an SDK-style
  conversion of a System.Web project can pass build, tests, and app smoke
  while silently breaking deployability. Publish-output verification belongs
  in any conversion checklist.

### 2026-07-24 · Sprint 5 · Minor frictions standing up the modern host (bundled)

- **Problem:** nothing sprint-threatening; three small surprises worth the
  honest record. (1) `dotnet new sln` on the .NET 10 SDK emits the new `.slnx`
  XML solution format by default -- D15's wording assumed `.sln` and was
  amended; scripts/CI had to reference `.slnx`. (2) The analyzer posture
  (latest-recommended + warnings-as-errors) collided with the suite's
  underscored test-name convention (CA1707) and Serilog's culture-sensitive
  console sink (CA1305) on day one -- resolved via a test-project-scoped
  .editorconfig carve-out and explicit `CultureInfo.InvariantCulture`.
  (3) `UserManager`'s ctor takes a non-nullable `IServiceProvider` under
  nullable reference types; the spike passes an empty built provider.
- **Where:** src/ scaffolding; SchemaParitySpikeTests; IdentityPasswordCompat
  SpikeTests.
- **Impact:** ~20 minutes total. Notably, the Sprint 4 toolchain-split
  prediction held exactly: the dotnet-CLI-only `src/` solution needed zero new
  shims, and all three gating spikes passed on first or second run.
- **Resolution:** fixed inline; D15 records the .slnx choice.
- **Report note:** tooling drift -- new-SDK defaults and analyzer strictness
  are cheap one-time costs on greenfield, in sharp contrast to the legacy
  lane's shim archaeology. The asymmetry itself is report material: the
  expensive half of a two-solution period is entirely the legacy half.

### 2026-07-24 · Sprint 4 · "--" in an XML comment broke CPM on CI (red PR run)

- **Problem:** a Copilot-run-1 comment reword reintroduced `--` inside an XML
  comment in `Directory.Packages.props` (invalid XML). NuGet then behaved as
  if the CPM file were absent: every project failed NU1015 ("no version
  specified") + NU1004 lock-file inconsistency on CI. Third occurrence of the
  same typo pattern this sprint (the writing convention "spaced double
  hyphen" leaking into XML), and the first to reach CI -- because the fix
  commit changed "only a comment" and was pushed without re-running restore.
- **Where:** commit 710ecfd; PR #9 checks red (build-and-test, nuget-audit).
- **Impact:** one red CI round, ~15 min.
- **Resolution:** fixed (comment reworded; repo-wide scan for `--` inside
  csproj/props comments now clean; locked restore re-proven locally before
  push). Lesson recorded: there is no such thing as a comment-only change to
  an MSBuild file; restore is the cheapest possible gate and runs before any
  push.
- **Report note:** tooling gap / process -- XML comment syntax rejects `--`,
  and MSBuild surfaces the parse failure as bewildering downstream NuGet
  errors rather than an XML error at the file.

### 2026-07-24 · Sprint 4 · No single toolchain can build the mixed net48/net10 solution

- **Problem:** the .NET 10 SDK declares `minimumMSBuildVersion` 18.0.0, so
  desktop MSBuild 17.14 (required by the System.Web/MVC 5 Web project, which
  the dotnet CLI cannot build) cannot compile a net10.0 target -- NETSDK1045
  with the resolver silently falling back to the 9.0 SDK. One solution, two
  toolchains, neither covers it end to end.
- **Where:** SocialGoal.Core multi-target (`net48;net10.0`), first Phase 1
  net10.0 attempt.
- **Impact:** ~45 min incl. the lock-file interaction below. Multi-targeting
  had to become opt-in (`-p:IncludeNet10=true`) so the desktop solution build
  stays net48; the net10.0 flavor builds via a dedicated dotnet-CLI step.
  Follow-on trap: the opt-in TFM changes the restore graph, which locked-mode
  restore rejects (and NU1005 blocks disabling the lock file ad hoc) -- solved
  by pointing the proof restore at a scratch `NuGetLockFilePath`.
- **Resolution:** fixed (conditional TargetFrameworks + separate proof step in
  BUILD.md/CI). Revisit when the legacy Web project retires (Sprint 11): the
  solution then builds wholly on the dotnet CLI.
- **Report note:** tooling gap -- the transition period of a Framework-to-.NET
  migration can sit across a hard toolchain boundary; plan for split builds
  rather than assuming one pipeline.

### 2026-07-24 · Sprint 4 · Katana 4 removes the dead Google OpenID call (forced source edit)

- **Problem:** the Katana 2.0.0 -> 4.2.3 security bump fails compilation on
  `app.UseGoogleAuthentication()` (Startup.Auth.cs:35): the parameterless
  OpenID 2.0 overload no longer exists (Google retired the protocol in 2014;
  the login was dead at baseline per the LMRR evaluation).
- **Where:** `App_Start/Startup.Auth.cs`, Web project build under Katana 4.2.3.
- **Impact:** ~10 min; one behavior-visible change -- the (nonfunctional)
  Google button disappears from the login page. Only source edit forced by the
  entire Sprint 4 package move.
- **Resolution:** fixed -- call commented out alongside the other provider
  stubs with a D5 pointer (Sprint 8 still decides whether a configured Google
  OAuth 2.0 login is wanted). Register + fresh login smoke green on 4.2.3
  (.AspNet.ApplicationCookie issued, authenticated redirect).
- **Report note:** dependency surprise -- a security bump deleting the API a
  dead feature sat on; the compiler, not the risk report, is what finally
  removed it.

### 2026-07-24 · Sprint 4 · View compilation (first ever) exposes two broken views

- **Problem:** the SystemWeb SDK enables MvcBuildViews-style view compilation
  by default; first-ever compile of the Razor views failed on
  `Views/Goal/Supporters.cshtml` and `Views/Goal/SupportersOfUpdate.cshtml`,
  both binding `ApplicationUser.UserId` -- a property that does not exist
  (IdentityUser has `Id`). These views crash at runtime in the legacy app
  whenever rendered; the legacy build never checked them (`MvcBuildViews=false`).
- **Where:** SDK conversion of `SocialGoal.Web.csproj`; errors at
  Supporters.cshtml(16,22), SupportersOfUpdate.cshtml(13,18).
- **Impact:** ~15 min. Conversion set `MvcBuildViews=false` for legacy parity;
  views are pinned behavior, rebuilt in Phase 2 (Sprint 9 slice), not fixed.
- **Resolution:** worked around (parity preserved); defect recorded as LMRR
  feedback (same class as the `_UpdateView.cshtml` SocialGoalUser cast).
- **Report note:** legacy defect + tooling gap -- uncompiled Razor views are a
  reservoir of latent runtime errors that no test or build step in the legacy
  toolchain ever exercised.

### 2026-07-24 · Sprint 4 · MSBuild.SDK.SystemWeb friction pair (CPM clash, VS-install web targets)

- **Problem:** two integration snags converting the Web project: (1) the SDK's
  implicit compiler PackageReferences carry versions, which NU1008-fails under
  central package management -- its own CPM auto-detection did not fire;
  (2) the SDK imports `Microsoft.WebApplication.targets` from the VS install
  path, absent on Build Tools/CI (Sprint 1's MSB4226 in new clothing).
- **Where:** `SocialGoal.Web.csproj` restore/build.
- **Impact:** ~30 min combined.
- **Resolution:** fixed -- `ApplySDKDefaultPackageVersions=false` +
  central pins for the two compiler packages; web targets fed from the pinned
  `MSBuild.Microsoft.VisualStudio.Web.targets` NuGet package via
  `WebApplicationsTargetPath`, so the shim is now a restore-time dependency
  and the CI-side `.buildtools` shim installs disappear.
- **Report note:** tooling gap -- the System.Web SDK-style ecosystem is
  community-maintained and needs these two glue decisions documented.

### 2026-07-24 · Sprint 4 · EF6 minor-version unification alone changes emitted DDL

- **Problem:** unifying EF 6.0.x -> 6.5.2 (no model change, no code change)
  flips `AspNetUsers.UserName` from `NULL` to `NOT NULL` in generated DDL. The
  Sprint 2 schema drift test caught it as a 1-line divergence from the baseline.
- **Where:** `SchemaSnapshotTests.SchemaBaseline_MatchesGeneratedModelDdl`,
  first full test run after the SDK-style/EF-6.5.2 conversion.
- **Impact:** ~30 min to diagnose and decide; baseline re-cut under D14. Would
  have been invisible without the Phase 0 schema referee -- on a live database
  this class of change surfaces as a surprise migration step at cutover.
- **Resolution:** fixed (D14: baseline re-cut; EF Core target schema inherits
  NOT NULL). LMRR feedback candidate recorded against R-004/R-012.
  Runtime confirmation same day: the app 500s against the pre-existing local
  dev DB ("model backing SocialGoalEntities has changed") because even the
  benign CreateDatabaseIfNotExists initializer runs a model-compatibility
  check -- on any long-lived environment this bump is a hard outage, not a
  silent drift. Local dev DB dropped and recreated under 6.5.2.
- **Report note:** dependency surprise / hidden behavior -- ORM version drift as
  a schema-change vector, distinct from the "version skew" risk as written.

### 2026-07-24 · Sprint 4 · Test repo-path logic broke on the SDK TFM output subfolder

- **Problem:** `SchemaSnapshotTests` located the repo root by counting `..`
  segments from the test assembly's directory, asserting it runs from
  `bin\Release`. SDK-style output adds a `net48` subfolder, so the tests
  silently resolved `docs/schema` to a nonexistent `source\docs\schema` and
  reported the committed baseline as "missing" (and self-primed a stray file
  there).
- **Where:** `source/SocialGoal.Tests/Data/SchemaSnapshotTests.cs` RepoPath.
- **Impact:** 2 spurious test failures entangled with a real schema finding
  (above); cost was mostly the care needed to separate the two.
- **Resolution:** fixed -- path now walks upward to a repo marker instead of
  assuming output depth.
- **Report note:** tooling gap -- output-layout assumptions embedded in test
  infra are a hidden cost of project-format modernization.

### 2026-07-24 · Sprint 3 · Absent authorization masked by runtime crashes (accidental gates)

- **Problem:** two mutating actions "reject" an unauthorized caller only because
  they crash, not because they check anything: `Account.Unfollow` throws
  `ArgumentNullException` (`DbSet.Remove(null)` when the caller-scoped follow row
  is absent) and `Group.CreateGoal` throws `NullReferenceException` for a
  non-member (null `GroupUser` dereferenced). A naive dynamic probe ("did the
  unauthorized call fail?") would read these as protected; they are not.
- **Where:** surfaced while writing the behavioral authz matrix
  (`source/SocialGoal.Tests/Authorization/`), invoking the real controllers.
- **Impact:** none to schedule -- turned into two precise `Assert.Throws` pins
  and an lmrr-feedback methodology note; matters for the Phase 2 rebuild (must
  not mistake the crash for a control).
- **Resolution:** pinned as-is (exceptions are outcomes; PIN, NEVER FIX).
- **Report note:** hidden behavior -- a class of finding only dynamic execution
  reveals, and one that can fool a dynamic check as easily as a static one.

### 2026-07-24 · Sprint 3 · EmailRequest actions persist, then throw at the session facade

- **Problem:** `EmailRequest.AddGroupUser`/`AddSupportToGoal` commit the
  join/support and delete the token, then throw `NullReferenceException` at
  `SocialGoalSessionFacade.Remove(...)` because `HttpContext.Current` is null
  out-of-request. The design doc anticipated deferring these to a service-level
  pin if the session facade proved unmockable; in practice the controller
  actions were reachable in-process and the persist-before-throw sequence is a
  cleaner characterization than a service-level proxy.
- **Where:** `EmailRequestAuthorizationMatrixTests`.
- **Impact:** none -- pinned both facts (mutation persists; action throws).
- **Resolution:** fixed/worked-as-hoped -- no session fabrication needed.
- **Report note:** hidden behavior -- an ambient-context dependency
  (`HttpContext.Current`) that fails a controller action *after* its side
  effects have committed; a partial-failure shape the rebuild's explicit
  DI removes.

### 2026-07-24 · Sprint 3 · net48 retarget breaks the hermetic build (targeting-pack root is single-valued)

- **Problem:** retargeting only SocialGoal.Tests to net48 (forced by the NUnit
  3/Moq 4.20 floor) failed the build with MSB3644: the proven Sprint 1 recipe
  passes one global `/p:TargetFrameworkRootPath` pointing at the pinned
  net45-only reference-assembly shim, and the net48 project inherits it and
  finds no v4.8 pack there. MSBuild accepts a single root, so a mixed-TFM
  solution can't point different projects at different pinned packs.
- **Where:** `docs/BUILD.md` recipe + `.github/workflows/legacy-ci.yml` build
  step; hit by the implementor during the Sprint 3 test-infra refresh.
- **Impact:** ~part of one implementor session; touched the shared build
  recipe (BUILD.md + CI), not just the Tests project.
- **Resolution:** fixed -- pin `Microsoft.NETFramework.ReferenceAssemblies.net48`
  alongside net45 and merge both packs into one `.buildtools\refasm` root;
  advisor approved the merged-root approach (minimal hermetic fix; the
  alternative -- relying on the runner's system-installed net48 pack -- gives
  up reproducibility).
- **Report note:** tooling gap -- mixed-framework transition states (exactly
  what a staged modernization creates) fight MSBuild's single targeting-pack
  root; any epic that upgrades test infra ahead of the app hits this.

### 2026-07-24 · Sprint 3 · No in-process HTTP test host exists for System.Web MVC 5

- **Problem:** the epic's Sprint 3 remediation ("HTTP-level pinning tests over
  every mutating action") assumes a test host that the legacy platform does
  not have. MVC 5 routes through System.Web; `Microsoft.Owin.Testing` cannot
  host it, ASP.NET Core `TestServer` does not apply, so the only true-HTTP
  option is out-of-process IIS Express automation with cookie + antiforgery
  scraping per actor -- unacceptably flaky for a ~30-action x 6-actor matrix
  in CI.
- **Where:** Sprint 3 planning; `docs/SocialGoal_Modernization_Epic.md` Sprint
  3 bullet 3.
- **Impact:** design detour before any Sprint 3 code; matrix approach had to
  be re-derived (reflection surface + controller-invocation behavioral layer).
  No schedule loss beyond the planning hour, but the remediation as written
  was not implementable.
- **Resolution:** worked around via D11 (two-layer matrix; optional IIS
  Express probe layer only if stable).
- **Report note:** spec gap / platform limitation -- remediation guidance
  written against modern-host assumptions; legacy System.Web hosts need a
  different (and cheaper) pinning strategy. LMRR feedback candidate for
  R-007-class recommendations.

### 2026-07-24 · Sprint 2 · OpenCover profiler silently produces empty coverage

- **Problem:** on the same commit, the pull_request-event CI run passed the
  coverage gate while the push-event run failed it with "SocialGoal.Data
  missing from coverage results" -- OpenCover's `-register:user` profiler
  failed to attach (its "No results" warning), tests all passed, and the empty
  coverage.xml flowed into a misleading gate error.
- **Where:** `.github/workflows/legacy-ci.yml` coverage step, hosted runner.
- **Impact:** one red CI run on a green commit; ~20 minutes to diagnose from
  logs.
- **Resolution:** fixed -- switched to registration-free `-register:path64`,
  added a module-data verification with one retry, and split the error
  messages so a profiler failure is no longer reported as a coverage-floor
  breach. Both event runs green on the following commit.
- **Report note:** tooling gap. Coverage-as-gate needs the collector's own
  health checked; "tests passed + no coverage" is a distinct failure mode
  from "coverage dropped," and conflating them sends investigation the wrong
  way.

### 2026-07-24 · Sprint 2 · LocalDB instance corrupted by an orphaned engine process

- **Problem:** first EF connect to `(localdb)\MSSQLLocalDB` hung the test
  process; the instance would not start (engine assertion Error 17066 in
  hkhost.cpp). `sqllocaldb start/delete/create` all failed -- an orphaned
  `sqlservr.exe` from the crashed instance still held the instance files
  (CopyFileW error 32).
- **Where:** implementor session, first characterization-test run.
- **Impact:** ~4 recovery iterations, two hung commands; delayed the first
  green run.
- **Resolution:** fixed -- killed the orphaned LocalDB `sqlservr.exe` (after
  distinguishing it from the machine's real MSSQLSERVER service process), then
  recreate/start succeeded. CI gets a `sqllocaldb` pre-start step for
  robustness.
- **Report note:** tooling gap. LocalDB state is machine-global and survives
  crashed test runs; characterization harnesses need instance-recovery
  awareness, not just connection strings.

### 2026-07-24 · Sprint 2 · EF 6.0.x hides its mapping API; store-space naming trap

- **Problem:** the mapping smoke tests could not use the C-S mapping types
  (`EntityContainerMapping` etc.) -- public only from EF 6.1; this solution
  pins EF 6.0.x. Table resolution had to go through store-space (SSpace)
  metadata. Second trap: store entity-set names are entity-type names while
  conceptual set names are DbSet property names (`Support` vs `Supports`),
  which cost an iteration.
- **Where:** `SocialGoal.Tests/Data/MappingSmokeTests.cs`.
- **Impact:** one extra test-debug iteration; no scope change.
- **Resolution:** worked around via SSpace metadata; naming pinned in tests.
- **Report note:** dependency surprise. Pre-6.1 EF6 pins limit standard
  model-introspection tooling; a modernization estimate touching EF 6.0.x
  should budget for it.

### 2026-07-24 · Sprint 2 · Crashed session left a divergent pushed branch

- **Problem:** a prior Sprint 2 attempt died on an API error after creating
  and pushing `sprint/s2-safety-net-1` with its own Sprint 1 gate-record
  commit (a2b6f58, ai-context only) -- parallel to the gate record that
  landed on master via PR #4. Starting Sprint 2 fresh hit "branch already
  exists" with a remote tracking a superseded commit.
- **Where:** git branch state at Sprint 2 kickoff.
- **Impact:** ~10 minutes to diagnose and reconcile; no content lost (the
  stray commit was a duplicate of the merged PR #4 record).
- **Resolution:** fixed -- verified the stray commit was ai-context-only and
  had no PR, hard-reset the branch onto merged master (c24f86e), force-pushed
  the replacement.
- **Report note:** tooling/process gap. AI sessions that crash mid-flow can
  leave pushed state behind; a session restart must reconcile remote branches
  against merged history before resuming, not assume a clean start.

### 2026-07-23 · Sprint 1 · Permission layer blocked delegated governance actions

- **Problem:** the epic's "protect master" bullet could not be executed:
  the branch-protection API call was blocked by the tool-permission
  classifier (repo-settings changes sit outside the delegated git
  permissions), and one routine `gh run list` was transiently blocked by the
  same layer minutes after identical calls succeeded.
- **Where:** `gh api PUT .../branches/master/protection`; PR-flow automation.
- **Impact:** minutes; one sprint bullet handed back to the operator (exact
  command in tasks.md). No scope change.
- **Resolution:** worked around -- operator action listed; run-status checks
  switched to `gh api` reads, which pass consistently.
- **Report note:** tooling/process gap. Delegation boundaries between an AI
  implementer and repo governance need to be explicit up front: "Claude may
  push and PR" does not imply "Claude may change repo settings," and the
  enforcement layer's judgment calls are not always predictable mid-flow.

### 2026-07-23 · Sprint 1 · Secret scan's first run flagged Microsoft's public key token

- **Problem:** the new gitleaks lane failed its first CI run with 5 "leaks" --
  all the same string, `b77a5c561934e089`, the standard .NET assembly
  `PublicKeyToken` in Web.config/App.config, matched by the generic-api-key
  rule across historical commits.
- **Where:** `.github/workflows/security.yml` secret-scan job; verified
  locally with the same gitleaks build.
- **Impact:** ~15 minutes; first security-lane run red on a false positive.
- **Resolution:** fixed -- `.gitleaks.toml` allowlist scoped to
  `PublicKeyToken=<16 hex>` on the matched line; local re-scan clean (76
  commits, 0 leaks).
- **Report note:** tooling gap. Secret scanners need framework-aware
  allowlists before they're credible gates on .NET Framework repos; the very
  first artifact a scanner meets in a legacy .NET tree is a public key token
  that looks like a key.

### 2026-07-23 · Sprint 1 · "Dead" Forms-auth scaffolding is load-bearing in three places

- **Problem:** the plan said remove the dead Web.Core Forms-auth scaffolding,
  but only `UserAuthenticationTicketBuilder.cs` is truly dead (fully commented
  out). `DefaultFormsAuthentication`/`IFormsAuthentication`/`SocialGoalUser`
  are referenced by ~30 test fixtures (principals built from
  `FormsAuthenticationTicket`), by `Bootstrapper.cs:38` as the assembly-scan
  anchor for Web.Core DI registrations, and by
  `Views/Group/_UpdateView.cshtml:154`, which casts `User.Identity` to
  `SocialGoalUser` at runtime. Bonus defect: under OWIN the identity is a
  `ClaimsIdentity`, so that view cast must throw `InvalidCastException`
  whenever the block renders -- the "am I the author" UI path in group updates
  is broken in the legacy app as committed.
- **Where:** Web.Core Authentication/Models; Tests; `_UpdateView.cshtml`.
- **Impact:** scope trim, no time lost beyond the survey; full removal of the
  types moves to Phase 2 (Web.Core retirement, Sprints 8/11). Removing them
  now would shred the 113-test safety net during the containment sprint.
- **Resolution:** worked around -- deleted only the truly dead file, replaced
  the Forms `<authentication>` remnant with `mode="None"` (behavior-preserving:
  `FormsAuthenticationModule` was already removed from the pipeline), left the
  referenced types in place. View-cast defect left as-is (behavioral
  reference); the rebuild slices replace it.
- **Report note:** hidden behavior + legacy defect. "Dead code" verdicts need
  reference-level verification, not file-level; and remnant auth types can
  keep compiling precisely because tests fake identity through them.

### 2026-07-23 · Sprint 1 · Legacy build needs two NuGet shims on modern tooling

- **Problem:** the 2014 solution no longer builds on stock modern tooling.
  VS2022 Build Tools ships no `Microsoft.WebApplication.targets` (MSB4226 from
  the Web csproj import), and the .NET 4.5 targeting pack is retired -- the
  machine's `Reference Assemblies\...\v4.5` folder exists but contains only XML
  doc stubs, so the build fails MSB3644 even though the folder looks installed.
- **Where:** `source/SocialGoal.sln` via MSBuild 17.14 (VS2022 Build Tools);
  hit while proving the reproducible build for Sprint 1.
- **Impact:** ~30 min diagnosis; shapes CI design (the same shims are needed on
  `windows-latest`); no scope change.
- **Resolution:** worked around with two pinned NuGet packages, no admin
  installs: `MSBuild.Microsoft.VisualStudio.Web.targets 14.0.0.3` via
  `/p:VSToolsPath` and `Microsoft.NETFramework.ReferenceAssemblies.net45 1.0.3`
  via `/p:TargetFrameworkRootPath`. Tests need the retired NUnit 2.x console
  (`NUnit.Runners 2.6.4`); modern runners cannot execute NUnit 2.6.3 suites.
  Full recipe in `docs/BUILD.md`. Result: build clean, 113/113 tests green.
- **Report note:** tooling gap (vendor retirement). A decade-old project's
  build now depends on archived shim packages; the "misleading stub folder"
  (targeting-pack directory present, assemblies absent) is the kind of trap
  that burns hours in a real engagement.

### 2026-07-23 · Pre-Sprint 1 · Copilot loop took 4 runs on a docs-only PR

- **Problem:** reaching a clean Copilot review took 4 runs even with no
  application code in the PR. The branch-guard hook alone needed two fix cycles
  (GNU-only `\b` portability; then verb-position false positives and missing
  PowerShell-tool coverage). Separately, the review-arrival poll first
  false-triggered on Claude's own thread reply, which GitHub records as a
  review object.
- **Where:** PR #1; `protect-master.sh`/`.ps1`; gh API polling.
- **Impact:** roughly an hour of iteration; no scope change.
- **Resolution:** fixed -- hooks are now tested twins (sh 9/9, ps1 6/6 pattern
  cases); poll filters to bot-authored reviews only.
- **Report note:** tooling gap, twice over. Guardrail automation is code and
  needs test cases and adversarial review like any code; and bot-review
  plumbing has its own edge cases (replies counted as reviews).

### 2026-07-23 · Pre-Sprint 1 · origin pointed at upstream; no Evincia remote existed

- **Problem:** the local repo's `origin` was `MarlabsInc/SocialGoal` (upstream,
  no push rights); a push or PR would have targeted a third party's public repo.
  No evincia-llc copy existed.
- **Where:** first run of the `pr-flow` skill (foundation PR).
- **Impact:** minutes of delay; required an operator decision on repo home and
  visibility.
- **Resolution:** fixed -- created public `evincia-llc/SocialGoal`, retargeted
  remotes (`origin` = evincia-llc, `upstream` = MarlabsInc), pushed baseline
  `master` + branch, raised PR #1 against evincia-llc `master`.
- **Report note:** tooling/process gap (environment assumption in the clone),
  not a legacy defect. Standing guard from here: never push or PR against
  `upstream`.
