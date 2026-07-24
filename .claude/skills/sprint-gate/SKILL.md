---
name: sprint-gate
description: Run a sprint exit-gate review for the SocialGoal epic -- verify each gate criterion with concrete evidence, enforce the sequencing law, record results, and report to the operator. Use at sprint end, before any next-sprint work starts.
---

# Sprint gate review

1. Read the sprint's exit gate in `ai-context/backlog.md` and its fuller wording
   in `docs/SocialGoal_Modernization_Epic.md`.
2. **Evidence per criterion.** Each criterion passes only on concrete evidence:
   a CI run link, test output, a committed file path, a documented answer. No
   criterion passes on assertion alone.
3. **Sequencing law check** (LMRR): Phase 0/1 gates must be green before any
   Phase 2 work is planned or briefed. Sprint 5's gate additionally requires D1
   and D2 to be DECIDED in `ai-context/decisions.md`.
4. **Record:** `backlog.md` status -> `done`, or `blocked(reason)` with the
   failing criterion; session-log entry in `tasks.md`; a `journal.md` entry for
   every gate failure (gate failures are exactly the friction the end-of-epic
   report needs).
5. **On PASS, update the public face:** refresh the Status section in
   `README.md` (gates passed, suite/coverage numbers, next sprint) and push an
   annotated gate tag (`s<n>-gate`) on the sprint's merged content commit.
   Add the sprint's row to the effort-actuals table in
   `ai-context/lmrr-feedback.md`.
6. **Report to operator:** table of criterion / evidence / verdict, overall
   pass or fail, and the next sprint's first three actions if passing.
