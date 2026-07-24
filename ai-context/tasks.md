# Tasks -- current state

**Phase:** Phase 0 (safety gate), Sprint 2
**Current sprint:** Sprint 2 (Safety net I) -- all deliverables implemented,
PR pending
**Branch state:** current branch `sprint/s2-safety-net-1` (from merged master
c24f86e; the crashed-session stray branch was reconciled -- see journal).
(update this line every session)

## Now (next actions, in order)

1. Operator: merge PR #5 (Sprint 2; Copilot loop complete -- 4 runs, run 4
   clean; security-reviewer PASS; all CI lanes green on head).
2. Run `sprint-gate` for Sprint 2 post-merge, then start Sprint 3 (Safety net
   II: authz matrix + CSRF characterization, test-infra refresh to NUnit 3)
   on `sprint/s3-safety-net-2`.
3. Deferred from foundation, now unblocked: create the
   `characterization-tests` skill from the proven LocalDB harness pattern
   (SetUpFixture lifecycle + FK-safe cleanup + store-metadata table
   resolution).

## Blocked / waiting

- None.

## Later (scheduled automation)

- Sprint 2: create `characterization-tests` skill once the LocalDB harness
  pattern is proven -- not before.
- Sprint 9: create `slice-migration` skill after the first vertical slice lands;
  reuse for the remaining six slices.

## Session log (newest first; 2-4 lines each)

### 2026-07-24 (Sprint 2, later) -- PR #5 + security review + Copilot loop
- security-reviewer on the sprint diff: PASS, zero findings. PR #5 raised;
  Copilot runs 1-3 produced 3 comments, all fixed (undisposed NewRepo
  factories -> TearDown disposal; triggers.md evidence-claim scoping; code
  span reflow). Run 4 clean.
- Mid-loop CI flake diagnosed and fixed: OpenCover -register:user attached no
  profiler on one run (empty coverage, tests green) -- switched to path64 +
  module-data verify/retry; journaled. All 4 CI runs green on head eb95135.
- Next: operator merges PR #5; then sprint-gate S2 and start Sprint 3.

### 2026-07-24 (Sprint 2) -- Safety net I: all deliverables in one session
- Recovered from a crashed prior session's stray pushed branch (journaled),
  then delegated the characterization suite to the implementor: harness
  (LocalDB, explicit Delete/Create lifecycle), 28 data-layer tests, mapping
  smoke tests -- 142/142 green locally AND on windows-latest (LocalDB works in
  CI with a pre-start step).
- Proven hazards recorded in the LMRR register: blind full-row Update, silent
  write loss across separate DatabaseFactory instances, dead configs
  (ApplicationUserConfiguration w/ HasMaxLength(1) bug intent,
  GoalUpdateConfiguration fully orphaned).
- Schema baseline of record committed (30 tables/28 FKs, drift + checksum
  tests); trigger unknown CLOSED in writing (0 triggers, empirical + D1
  by-construction). Coverage in CI (OpenCover): 50.7% line / 26.8% branch
  overall, Data 72% (was 0) -- floor enforced. Next: PR + Copilot loop.

### 2026-07-23 (Sprint 1 gate) -- PASSED, all five criteria on evidence
- Gate review post-merge, all evidence at commit cd75621: CI green on master
  (legacy-ci run 30052286151, security 30052286169); zero DropCreateDatabase
  references in source; source/SocialGoal/Web.config has
  elmah.mvc.requiresAuthentication=true + allowedRoles=Admin and
  Feature.ImageUrlImport=false (+ pinning test); SBOM at
  docs/security/sbom-nuget.cdx.json.
- Operator merged #2/#3 and branch protection went live (applied via gh api at
  operator request; 4 required checks, PR-only, no force push/deletion).
- Sequencing law: no Phase 2 work briefed; Sprint 2 (Phase 0) may start.

### 2026-07-23 (Sprint 1, later) -- PR #3 + security review + Copilot loop
- security-reviewer agent on the sprint diff: PASS; 2 LOW supply-chain items
  fixed (sha256-pinned gitleaks, retire pinned), 1 INFO accepted with reason
  (gitleaks v8.18 ORs allowlist conditions -- paths would widen suppression).
- PR #3 raised; Copilot runs 1-3 produced 6 comments, all fixed (config-comment
  wording, specific disabled-import message, stale BUILD.md note, POSIX
  whitespace class, restore-failure gate in nuget-audit, SSRF-flag pinning
  test -- suite now 114/114). Run 4 clean. Effort actuals row added.
- Next: operator merges #2 then #3 + enables branch protection; then run
  `sprint-gate` for Sprint 1 and start Sprint 2.

### 2026-07-23 (Sprint 1) -- Containment + reproducible build, all deliverables
- Proved the legacy build end to end (restore, MSBuild 17 + two NuGet shims for
  retired tooling, NUnit 2.6.4 console): 113/113 green, recipe in `docs/BUILD.md`;
  legacy-ci workflow green on first run. Journaled the toolchain friction.
- Containment: initializer config-switched (DropCreate deleted outright; live
  proof via fresh DB + seeded lookups), ELMAH locked (verified 401->login),
  Forms-auth remnant removed (scaffolding found load-bearing in 3 places --
  journaled, R-006 nuance), URL import feature-flagged off (default).
- SBOM (CycloneDX, 41 pkgs) + SCA baselines committed (8 vulnerable NuGet pkgs,
  17 vendored JS files); security.yml = scan of record (gitleaks/nuget/retire,
  new-vs-baseline gates). Gitleaks false-positived on .NET PublicKeyToken;
  allowlisted and journaled. 13 golden-path screenshots + behavior notes.
- Tagged `legacy-baseline` (42cfdb4). Branch protection blocked by permissions
  -- handed to operator. All CI lanes green at 5908770.

### 2026-07-23 (later) -- POC vision, D1/D10, feedback register
- Operator merged PR #1; framed the epic as an Evincia POC: the modernization
  itself validates/corrects the LMRR. Created `lmrr-feedback.md` (seeded with
  evaluation-phase confirmations, the R-006 correction candidate, six missed
  candidates as engine-predicate input, effort table).
- D1 DECIDED (no live users/DB -- live-data rigor track dropped); D10 DECIDED
  (pragmatic PR granularity; guardrails retained). PR #2 raised; Copilot loop:
  runs 1-3 found 5 consistency-class comments (all fixed), run 4 clean.
- D2 DECIDED (Azure App Service Linux, private via Easy Auth + IP restrictions;
  cascades D8/D9/OIDC) appended to PR #2 with a grep-first cross-ref sweep;
  run 5 clean. Sprint 5 decision gate fully cleared -- PR #2 ready for merge.

### 2026-07-23 -- Evaluation, epic, foundation
- Evaluated Evincia LMRR (primary spec), secondary gap report, and full codebase
  survey; all findings reconciled (see `context.md` for the durable facts).
- Epic drafted as 4.8.x scope, then restructured per operator decision D0 to a
  single 14-sprint .NET 10 epic (`docs/SocialGoal_Modernization_Epic.md`).
- Created foundation: root `CLAUDE.md`, `ai-context/` (this folder),
  `.claude/rules/modernization.md`, .gitignore entries (committed later this
  session via PR #1; see below).
- Governance added per operator: PR review + security scan before merge, Copilot
  double-run policy, git permissions (Claude branches/commits/pushes; operator
  merges), `journal.md` for problems/roadblocks, `implementor` agent (Opus) with
  Fable as advisor.
- Automation established: skills (`pr-flow`, `session-close`, `sprint-gate`),
  `security-reviewer` agent, master-branch guard hook, settings.json read denies
  for build artifacts. Deferred: characterization-tests skill (Sprint 2),
  slice-migration skill (Sprint 9).
- Created public `evincia-llc/SocialGoal` (origin; MarlabsInc kept as
  `upstream`, never a push/PR target), raised PR #1 with the foundation.
  Copilot run 1: one comment (stale branch-state line in this file) -- fixed.
