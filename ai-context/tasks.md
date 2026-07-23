# Tasks -- current state

**Phase:** pre-Sprint 1 (foundation)
**Current sprint:** none started
**Branch state:** all foundation files untracked on `master`; nothing committed yet

## Now (next actions, in order)

1. Operator: review/merge the foundation (this folder, `CLAUDE.md`,
   `.claude/rules/`, epic doc) via feature branch + PR.
2. Operator: answer D1 (live data?) and D2 (hosting) in `decisions.md` -- both
   gate Sprint 5; D1 shapes Sprint 2's database work.
3. Start Sprint 1 (containment + reproducible legacy build) per
   `ai-context/backlog.md` -- first items: feature branch, legacy CI workflow,
   initializer disable, ELMAH lockdown.

## Blocked / waiting

- Sprint 5 exit gate blocked on D1, D2 (not urgent yet).

## Later (scheduled automation)

- Sprint 2: create `characterization-tests` skill once the LocalDB harness
  pattern is proven -- not before.
- Sprint 9: create `slice-migration` skill after the first vertical slice lands;
  reuse for the remaining six slices.

## Session log (newest first; 2-4 lines each)

### 2026-07-23 -- Evaluation, epic, foundation
- Evaluated Evincia LMRR (primary spec), secondary gap report, and full codebase
  survey; all findings reconciled (see `context.md` for the durable facts).
- Epic drafted as 4.8.x scope, then restructured per operator decision D0 to a
  single 14-sprint .NET 10 epic (`docs/SocialGoal_Modernization_Epic.md`).
- Created foundation: root `CLAUDE.md`, `ai-context/` (this folder),
  `.claude/rules/modernization.md`, .gitignore entries. Nothing committed;
  awaiting feature-branch/PR instruction.
- Governance added per operator: PR review + security scan before merge, Copilot
  double-run policy, git permissions (Claude branches/commits/pushes; operator
  merges), `journal.md` for problems/roadblocks, `implementor` agent (Opus) with
  Fable as advisor.
- Automation established: skills (`pr-flow`, `session-close`, `sprint-gate`),
  `security-reviewer` agent, master-branch guard hook, settings.json read denies
  for build artifacts. Deferred: characterization-tests skill (Sprint 2),
  slice-migration skill (Sprint 9).
