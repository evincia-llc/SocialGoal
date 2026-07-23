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
