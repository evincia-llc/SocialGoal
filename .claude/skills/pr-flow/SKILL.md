---
name: pr-flow
description: Prepare and push a reviewable PR branch for the SocialGoal epic following repo governance (tests, security scan, Copilot double-run policy, operator-only merge). Use when work is ready to leave the working tree, or when the operator says "PR this".
---

# PR flow

1. **Branch.** Never on `master`. Name: `<type>/s<sprint>-<slug>` (e.g.
   `feat/s1-ci-workflow`, `test/s2-data-characterization`, `docs/epic-foundation`).
2. **Tests.** Run the suites relevant to the change. Failures stop the flow; do
   not push red without operator sign-off recorded in `ai-context/tasks.md`.
3. **Security scan.** CI security lane once Sprint 1 lands; until then run the
   scan manually and note the result for the PR body.
4. **Review.** For changes touching controllers, authorization, data access,
   uploads, or tokens: run the `security-reviewer` agent on the diff first and
   resolve findings.
5. **Commit.** Message references sprint, backlog item, and any decision IDs
   (e.g. `Sprint 1: disable destructive initializer (backlog S1, D0)`).
6. **Push** the feature branch. **Never merge** -- operator-only.
7. **PR body template:**
   - Summary (what and why, one paragraph)
   - Sprint / backlog ref / spec refs (LMRR R-ids, gap #)
   - Test evidence (suite, counts, link to CI run)
   - Security scan result
   - Copilot checklist: run 1 findings -> per-comment action (fix or ignore,
     with reason) -> run 2 result (skip run 2 only if run 1 was clean)
8. Raise the PR with the template and request Copilot review (run 1). Claude
   may raise PRs and drive the Copilot iterations; only the operator merges.
9. Log any friction hit during the flow in `ai-context/journal.md`.
