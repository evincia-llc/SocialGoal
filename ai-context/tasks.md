# Tasks -- current state

**Phase:** pre-Sprint 1 (foundation)
**Current sprint:** none started
**Branch state:** PR #1 merged to `master` (3b3b289). Current branch
`docs/poc-vision-governance` -- PR #2 pending operator merge (update this line
every session)

## Now (next actions, in order)

1. Operator: review and merge PR #2 (POC vision, D1/D10 decisions, LMRR
   feedback register).
2. Start Sprint 1 (containment + reproducible legacy build) on
   `sprint/s1-containment` per `ai-context/backlog.md` -- first items: legacy CI
   workflow, initializer disable, ELMAH lockdown.
3. Operator: answer D2 (hosting target) in `decisions.md` before Sprint 5.

## Blocked / waiting

- Sprint 5 exit gate blocked on D2 only (D1 decided 2026-07-23).

## Later (scheduled automation)

- Sprint 2: create `characterization-tests` skill once the LocalDB harness
  pattern is proven -- not before.
- Sprint 9: create `slice-migration` skill after the first vertical slice lands;
  reuse for the remaining six slices.

## Session log (newest first; 2-4 lines each)

### 2026-07-23 (later) -- POC vision, D1/D10, feedback register
- Operator merged PR #1; framed the epic as an Evincia POC: the modernization
  itself validates/corrects the LMRR. Created `lmrr-feedback.md` (seeded with
  evaluation-phase confirmations, the R-006 correction candidate, six missed
  candidates as engine-predicate input, effort table).
- D1 DECIDED (no live users/DB -- live-data rigor track dropped); D10 DECIDED
  (pragmatic PR granularity; guardrails retained). PR #2 raised; Copilot loop
  took 4 runs (5 comments, all consistency-class), run 4 clean -- ready for
  operator merge.

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
