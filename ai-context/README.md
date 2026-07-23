# ai-context/ -- working memory for the modernization epic

Durable, checked-in session-to-session context. Root `CLAUDE.md` points here; these
files are the state, `docs/SocialGoal_Modernization_Epic.md` is the plan, and the
two reports in `docs/` are the spec. Keep each file current or delete it -- stale
context is worse than none.

| File | Role | Update discipline |
|---|---|---|
| `context.md` | Project snapshot: what this is, key codebase facts, spec pointers | When facts change (rare); verify before trusting details older than a phase |
| `decisions.md` | Decision log: DECIDED entries and the open register (currently D2-D9) | Append-only entries; flip status when the operator decides; never delete history |
| `tasks.md` | Current sprint state, in-flight work, next action, session log | **Every working session**, at session end |
| `backlog.md` | Sprint checklist with status and exit gates | When a sprint item or gate changes state |
| `journal.md` | Problems/roadblocks log -- raw material for the end-of-epic report | The moment a problem is hit, not retrospectively |
| `lmrr-feedback.md` | Per-finding LMRR validation ledger + effort actuals (the POC instrument) | When implementation confirms/corrects/contradicts a finding; effort at each sprint gate |

Rules:

- Scope lives in the epic doc; `backlog.md` mirrors status only. If scope must
  change, record why in `decisions.md`, edit the epic doc on a feature branch, then
  sync the backlog.
- Decisions get an ID, a date, an owner, and a status (OPEN / DECIDED / SUPERSEDED).
  Code must not depend on an OPEN decision.
- Session log entries in `tasks.md` are two to four lines: what moved, what
  surprised, what's next. Not a diary.
- Nothing engine-derived or secret goes in this folder.
