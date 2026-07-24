# Tasks -- current state

**Phase:** PHASE 1 (foundation retarget); Sprint 5 work COMPLETE, PR loop next
**Current sprint:** Sprint 5 (modern .NET 10 host + spikes) -- all
deliverables done, all three gating spikes PASSED
**Branch state:** `sprint/s5-modern-host` pushed; all three CI lanes green on
first push (legacy-ci, modern-ci NEW, security). Security-reviewer pass +
PR/Copilot loop in flight this session. (update this line every session)

## Now (next actions, in order)

1. Finish the Sprint 5 PR loop: security-reviewer findings (if any), raise PR
   via pr-flow, Copilot iterate-until-clean.
2. Operator: review/merge the Sprint 5 PR. Flagged for explicit operator
   attention at review: D15 (src/ layout, .slnx, NUnit 4), ADR-001 PROPOSED
   status, and the deliberately-unfixed legacy publish-content regression
   (journal 2026-07-24 -- legacy publish is code-only under the SystemWeb SDK;
   no consumer exists, legacy retires S11; overturn = add Content globs).
3. Then sprint-gate for Sprint 5 / Phase 1 exit (LMRR Phase 2 entry:
   ADR + proven auth/data approach + slice running -- all evidence in
   docs/adr/ADR-001-modern-host.md and src/SocialGoal.Web.Tests).

## Blocked / waiting

- None.

## Later (scheduled automation)

- Sprint 9: create `slice-migration` skill after the first vertical slice lands;
  reuse for the remaining six slices.
- (done 2026-07-24) `characterization-tests` skill minted at S2 gate close from
  the proven harness.

## Session log (newest first; 2-4 lines each)

### 2026-07-24 (Sprint 5) -- modern host + all three gating spikes PASSED
- New src/ solution (D15: .slnx, dotnet-CLI-only, own CPM/locks, NUnit 4):
  ASP.NET Core .NET 10 MVC host (Serilog, health, Data Protection, analyzers
  as errors) + modern-ci lane (locked restore, format, tests, zero-tolerance
  SCA) -- green on first hosted run. ADR-001 PROPOSED with spike evidence.
- Spikes: (1) EF Core schema parity proven by live catalog diff vs baseline
  (35 cols/7 FKs exact incl. TPH artifacts + shadow FK columns; index delta
  pinned); (2) Identity 1.0 hash (real legacy assembly) verifies under Core
  Identity + v3 rehash persisted via UserManager over legacy-shaped table;
  (3) goal-detail slice over WebApplicationFactory from baseline schema --
  epic's first true HTTP test (D11 follow-up). Suite 10/10.
- Web.Release.config publish proof (delegated): transforms PASS; found the
  SystemWeb SDK publishes code-only (no .cshtml/static globs) -- documented
  (journal/LMRR/BUILD.md), deliberately unfixed, operator may overturn.
- 3 LMRR entries (R-004 mapping-fidelity classes, R-005 confirmed, publish
  risk class) + 2 journal entries. Next: security review, PR, Copilot loop.

### 2026-07-24 (Sprint 4 gate + close-out) -- PASSED
- Gate verified independently @ merged 40fa23a (both CI lanes green): 7/7 csproj
  SDK-style; CPM with EF 6.5.2/Katana 4.2.3/Newtonsoft 13.0.4; CI step "Prove
  SocialGoal.Core on .NET 10" = success (net10.0 compile, not just declared);
  187/187 suite green; nuget-audit 8->2. Sequencing law intact (Phase 1, no
  Phase 2 briefed).
- Close-out: backlog S4 done; `s4-gate` tag; README status (Phase 1 underway,
  first .NET 10 assembly); register gains R-005/R-008 confirmed+remediated and
  R-001 feasibility-proven entries. S4 effort row already present (~half day).

### 2026-07-24 (Sprint 4) -- foundation retarget, all deliverables in one session
- All 7 projects SDK-style (D13): six on Microsoft.NET.Sdk, Web on
  MSBuild.SDK.SystemWeb/4.0.107 (web targets = pinned NuGet dep, CI shims
  gone); whole solution net45->net48; CPM + committed lock files, locked-mode
  restore in CI. Six-project conversion delegated to implementor; Web + CI by
  Fable.
- EF unified 6.5.2: drift test caught UserName NULL->NOT NULL (D14 re-cut;
  runtime-confirmed vs stale dev DB). Newtonsoft 13.0.4; Katana 4.2.3 (one
  forced edit: dead Google OpenID call removed, D5 note); content-only
  packages dropped; audit baseline 8->2 + package-bump-analysis.md for the
  rest. Core multi-targets net10.0 (opt-in; .NET 10 SDK needs MSBuild 18 --
  toolchain split journaled). First-ever view compile exposed 2 broken views
  (LMRR missed-candidate; MvcBuildViews stays false for parity).
- Proof: build 7/7 shimless, 187/187, register/login + golden-path pages green
  on IIS Express, ELMAH lock intact. 4 journal entries, 3 LMRR entries, S4
  effort row. Next: security-reviewer + PR.

### 2026-07-24 (Sprint 4 start) -- ratifications recorded, awaiting go
- Operator ratified D11 (matrix test level) and confirmed the multi-user
  screenshot golden-paths deferral -- recorded as D11 status update + new D12.
- Branch `sprint/s4-foundation-retarget` cut from master @ c030de6 (PR #8
  merged). Sprint work gated on /effort auto confirmation per ritual.

### 2026-07-24 (Sprint 3 gate + close-out) -- PASSED; PHASE 0 COMPLETE
- Gate evidence @ merged 1359be1 (CI green post-merge): 187/187 matrix suite in
  CI; all three R-007 seams lit -- data 87.6% line, auth enforcement surface
  pinned (149-action census + 27 behavioral tests + inert filters proven dead),
  triggers closed (S2). Structural-work sign-off granted; D11 + golden-paths
  deferral surfaced for operator ratification.
- Close-out: backlog S3 done; `s3-gate` tag; README status refresh + 3
  evincia.co citation links (URLs verified live); sprint-start ritual added to
  CLAUDE.md (effective Sprint 4); S3 + Phase 0 effort rows (~2.5 days total vs
  "a few weeks").

### 2026-07-24 (Sprint 3) -- Safety net II: authz/CSRF matrix + test-infra refresh
- Test infra: NUnit 2.6.3->3.14, Moq 4.1->4.20.72, Tests retargeted net48
  (CI targeting-pack merge under one refasm root; net48 friction journaled).
  Enforcement-surface tests: full 149-action reflection census (verified vs
  source), 7/25 CSRF split, 23 mutating GETs (corrects report's "~17"), inert
  filters proven dead. Behavioral matrix: 27 controller-invocation tests over
  LocalDB proving BOLA persists real mutations + Admin-flag-never-gates. Suite
  144->187, all green locally (build + OpenCover verified).
- D11 recorded (matrix test level: reflection surface + controller-invocation,
  no in-proc HTTP host for System.Web MVC 5 -- flagged for operator ratify).
  New findings to LMRR: 23-GET correction, EditProfile cross-user write, two
  accidental gates (crashes not authz), JoinGroup open-join, token bearer-secret.
  Matrix summary doc (Phase 2 enforcement spec) + coverage S3 (~55% overall,
  auth seam characterized) committed.
- security-reviewer on the diff: PASS, zero findings (no production code
  touched). PR #7 raised (had to target evincia-llc explicitly -- gh defaulted
  to the MarlabsInc upstream). All 4 CI checks green on windows-latest incl. the
  first CI run of the net48/NUnit3 targeting-pack merge. Copilot run 1: 3
  comments -- BUILD.md count 144->187 fixed, tasks.md "unpushed" fixed, the
  FilterConfig "won't compile" comment rejected (it's an ancestor namespace;
  compiles+green in CI) with a thread reply. Run 2 clean. Also gitignored the
  coverage output. Awaiting operator merge, then sprint-gate.

### 2026-07-24 (Sprint 2 gate + close-out) -- PASSED, all criteria on evidence
- Gate evidence @ merged f618d84 (both CI lanes green post-merge): Data 72%
  line w/ enforced floor (`docs/coverage-baseline.md`); schema baseline +
  checksum/drift tests (`docs/schema/`); triggers CLOSED in writing (0
  empirical + D1 by-construction); OpenCover instrumented in legacy-ci.
- Close-out (this branch): backlog S2 done; tags `s1-gate`/`s2-gate` pushed;
  README rewritten (POC framing, production disclaimer, provenance, status);
  `characterization-tests` skill minted from the proven harness; sprint-gate
  skill gains README-status/tag/effort step; S2 effort row added (~1 day vs
  2-week plan).

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
