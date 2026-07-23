# Tasks -- current state

**Phase:** Phase 0 (safety gate), Sprint 1
**Current sprint:** Sprint 1 (containment + reproducible legacy build) --
deliverables complete, PR pending
**Branch state:** current branch `sprint/s1-containment` (branched from PR #2
head so ai-context edits don't conflict; its diff collapses when #2 merges).
PR #2 still pending operator merge (update this line every session)

## Now (next actions, in order)

1. Operator: review and merge PR #2, then the Sprint 1 PR (raised from
   `sprint/s1-containment`; includes #2's commits until #2 merges).
2. Operator: enable master branch protection (Claude's API call was
   permission-blocked). Required checks `build-and-test`, `secret-scan`,
   `nuget-audit`, `retire-js`:
   `gh api -X PUT repos/evincia-llc/SocialGoal/branches/master/protection --input protection.json`
   (epic Sprint 1 bullet; any settings route is fine).
3. Run the `sprint-gate` review for Sprint 1, then start Sprint 2 (data-layer
   characterization, schema snapshot, trigger question) on `sprint/s2-safety-net-1`.

## Blocked / waiting

- Master branch protection: needs operator (permission classifier blocks repo
  settings changes from Claude).

## Later (scheduled automation)

- Sprint 2: create `characterization-tests` skill once the LocalDB harness
  pattern is proven -- not before.
- Sprint 9: create `slice-migration` skill after the first vertical slice lands;
  reuse for the remaining six slices.

## Session log (newest first; 2-4 lines each)

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
