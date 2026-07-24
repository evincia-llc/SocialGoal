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
