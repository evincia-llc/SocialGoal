# SocialGoal Modernization

Legacy ASP.NET MVC 5 / .NET Framework 4.5 reference app (MarlabsInc/SocialGoal) being
modernized to ASP.NET Core MVC on .NET 10 under a 14-sprint epic. This repo is the
implementation project; the risk reports in `docs/` are the specification.

**This epic is an Evincia POC** (see `ai-context/context.md`, Goal/vision): the
journal, the LMRR feedback register, and effort actuals are deliverables on par
with the code. Log friction and LMRR confirmations/corrections as they happen.

## Read first, every session

1. `ai-context/tasks.md` -- current sprint, in-flight work, next action.
2. `ai-context/context.md` -- project state and key codebase facts.
3. `docs/SocialGoal_Modernization_Epic.md` -- the governing plan (14 sprints, 4 phases).

## Specification hierarchy (do not invert)

1. **Primary:** Evincia LMRR, `docs/Evincia-Sample-LMRR-SocialGoal.pdf` (risk register
   R-001..R-016, phase sequencing, readiness scoring). Governs scope and order.
2. **Secondary (gap areas only):** `docs/SocialGoal_Modernization_Risk_Report.md`.
   Used exclusively for four gaps the LMRR does not cover: broken object-level
   authorization, state-changing GETs/CSRF, SSRF in image URL import, destructive
   DB initializer. Everything else in that file is background, not spec.
3. The epic doc is the operational plan derived from both. Scope changes go through
   `ai-context/decisions.md`, not ad-hoc edits.

## Hard rules

- **Safety net precedes structural change** (LMRR sequencing law). No Phase 2 work
  (EF Core, Identity, System.Web rebuild) before the Phase 0/1 exit gates in the
  epic doc are met.
- **Feature branches only. Never commit to `master`**, for any change class.
  Merges are operator-only (see Workflow, permissions, and roles).
- **Decisions are recorded before code relies on them.** Open decisions (D1-D9) and
  new ones land in `ai-context/decisions.md` with status and date. D1 (live data?)
  and D2 (hosting) gate Sprint 5.
- **Legacy code is a behavioral reference, not a trusted design.** Never port
  verbatim: naked-ID authorization, GET mutations, the `GetImageFromUrl` fetch,
  `DropCreateDatabaseIfModelChanges`, the inert security filters.
- **Update `ai-context/tasks.md` at the end of every working session** (state, next
  action, surprises). Update `ai-context/backlog.md` when sprint status changes.
- No secrets in source or config; environment/secret store only.
- Engine-derived Evincia data never gets committed here (cross-project rule).

## Workflow, permissions, and roles

- **Git permissions:** Claude may create feature branches, commit, push, raise
  PRs, and run the Copilot review iterations (delegated by the operator
  2026-07-23). **Claude never merges -- only the operator merges.**
- **Branching strategy (D10):** no PR-per-work-item ceremony. PR granularity is
  pragmatic -- a sprint, a workstream, or a slice, sized by what reviews
  sensibly (`sprint/s<n>-<slug>` naming). Large PRs fine when coherent; small
  ones fine too. Keep in-branch commits small and labeled -- bisect/rollback
  granularity lives at commit level.
- **Every PR passes code review and a security scan before merge.** The CI
  security lane (SCA/secret scan, a Sprint 1 deliverable) is the scan of record;
  until it exists, the scan is run manually and noted on the PR.
- **Copilot reviews every PR until clean.** A clean first run needs no second.
  Otherwise: address findings, re-request, and keep iterating until a run comes
  back with no comments or only trivial ones (operator rule 2026-07-23). Claude
  decides per comment whether to fix or ignore, stating the reason, and has
  permission to reply in Copilot conversation threads.
- **Access:** Claude has full read access to all documents, journals, logs, and
  markdown files in this repo.
- **Model roles (within one session):** the main session runs Fable 5 as
  advisor, orchestrator, and reviewer. Routine, well-scoped implementation is
  delegated to the `implementor` subagent, pinned to Opus
  (`.claude/agents/implementor.md`). Difficult or ambiguous implementation is done
  by Fable directly in the main loop.
- **Journal every problem and roadblock** in `ai-context/journal.md` at the moment
  it is hit, not retrospectively. The journal is the raw material for the
  end-of-epic report on the problems discovered while modernizing legacy
  SocialGoal.
- **LMRR feedback:** when implementation confirms, corrects, or contradicts an
  LMRR finding -- or surfaces something the LMRR missed -- record it in
  `ai-context/lmrr-feedback.md` with the R-id and evidence. Tag journal entries
  with R-ids where applicable.

## Build and test (legacy solution, pre-Sprint 4)

- Solution: `source/SocialGoal.sln` -- 7 projects, all .NET Framework 4.5,
  VS2013-era csproj. Requires Windows + MSBuild + NuGet restore.
- Tests: `source/SocialGoal.Tests` -- NUnit 2.6.3 + Moq 4.1 (~116 tests, controller
  fixtures only). Needs an NUnit 2.x-compatible runner until the Sprint 3 test-infra
  refresh.
- Database: EF6 Code First against local SQL (`Data Source=.\`, catalog
  `SocialGoal`, integrated security -- `source/SocialGoal/Web.config`).
  WARNING: as committed at baseline, startup registers a drop-if-model-changes
  initializer (`Global.asax.cs:16`). Sprint 1 removes it; until then never point
  the app at a database you care about.
- Reproducible CI build is a Sprint 1 deliverable; exact toolchain steps land in
  `docs/BUILD.md` when proven. Do not guess build commands into this file.

## Repo layout

- `source/` -- legacy solution (Web, Web.Core, Core, Model, Data, Service, Tests).
- `docs/` -- specifications, epic, build notes.
- `ai-context/` -- working memory across sessions (see its README for file roles).
- `.claude/` -- rules (path-scoped .NET conventions), skills (`pr-flow`,
  `session-close`, `sprint-gate`), agents (`implementor`, `security-reviewer`),
  hooks (master-branch guard), settings (build-artifact read denies).

## Token discipline

- `source/SocialGoal/Scripts/` and `Content/` are ~100k lines of vendored/minified
  libraries. Never read vendored library files into context; read only
  `BundleConfig.cs` and app-specific files. Delegate broad code exploration to
  subagents; keep the main loop for decisions and review.
