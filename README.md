# SocialGoal Modernization POC

An [Evincia](https://www.evincia.co) proof of concept: take a real, abandoned
.NET Framework 4.5 / ASP.NET MVC 5 application, modernize it to .NET 10 under
the guidance of an Evincia Legacy Modernization Risk Report (LMRR), and record
everything the work proves, disproves, or teaches about that report.

The modernization is real. The purpose is the evidence.

## Not for production

**Do not deploy this application -- baseline or modernized -- to production.**

* The legacy baseline contains known security defects (broken object-level
  authorization, CSRF gaps, an SSRF path), documented deliberately and in
  detail in [`docs/`](docs/) as part of the methodology. Several remain in the
  code until the sprints that rebuild them.
* There is no QA program, no client acceptance testing, and no hardening
  certification. That will remain true at the end: the finished .NET 10
  application demonstrates a methodology, not a shippable product.
* Every database this project touches is disposable sample data.

## Why this exists

Evincia's [Modernization Shield](https://www.evincia.co/modernization-shield.html)
produces LMRRs: evidence-based risk reports on legacy codebases, built from
static analysis plus senior-architect review. This repo asks the follow-up
question: **if you actually perform the modernization the report scoped, what
does the experience prove?**

Three instruments capture the answer, all versioned in this repo:

| Instrument | File | What it records |
|---|---|---|
| Journal | `ai-context/journal.md` | Every problem and roadblock, logged when it happens |
| LMRR feedback register | `ai-context/lmrr-feedback.md` | Per-finding validation: confirmed, corrected, missed; candidate detection signals |
| Effort actuals | `ai-context/lmrr-feedback.md` | Measured effort per sprint vs the report's illustrative estimates |

The subject report is the sample LMRR for this codebase:
[`docs/Evincia-Sample-LMRR-SocialGoal.pdf`](docs/Evincia-Sample-LMRR-SocialGoal.pdf),
also published at
[evincia.co/sample-lmrr-socialgoal](https://www.evincia.co/sample-lmrr-socialgoal.html).

## Status

Updated at each sprint gate. Gate tags: `s1-gate`, `s2-gate`, ...

| | |
|---|---|
| Plan | 14 sprints, 4 phases: [`docs/SocialGoal_Modernization_Epic.md`](docs/SocialGoal_Modernization_Epic.md) |
| Phase | **Phase 2 underway** -- EF6 &rarr; EF Core: the full 30-table model is mapped at exact schema parity to the baseline, governed by one Baseline migration with a CI-gated drift check (Sprint 6). Auth re-platform + the web rebuild follow. |
| Gates passed | Sprint 1 (containment + reproducible build), Sprint 2 (data-layer characterization, schema baseline, trigger unknown closed), Sprint 3 (authorization/CSRF matrix, NUnit 3), Sprint 4 (SDK-style conversion, EF unified at 6.5.2, vuln audit 8&rarr;2), Sprint 5 (modern .NET 10 host + three gating spikes), Sprint 6 (EF Core model + Baseline migration, schema parity through `Migrate()`) |
| Suite | Legacy: 187 tests green (data layer 87.6%; all 149 controller actions' enforcement surface pinned, `docs/security/authorization-matrix.md`). Modern: 30 tests green in `modern-ci` (schema parity, migration drift, EF Core data behavior). |
| Next | Sprint 7: async query/service layer over EF Core, retire the repository/UnitOfWork ceremony, port the remaining characterization tests |
| LMRR readiness trajectory | 44/100 (Red) at baseline, target ~88 (Green) after Phase 3 |

## How the work is organized

* `docs/SocialGoal_Modernization_Epic.md` -- the governing plan; every LMRR
  risk has an explicit disposition.
* `ai-context/` -- cross-session working memory: state, decisions, backlog,
  journal, feedback register.
* One PR per coherent chunk of work; every PR carries an AI code-review loop
  iterated until clean, and CI gates (build + tests, secret scan, dependency
  audits) are required on `master`.
* `docs/BUILD.md` -- the proven legacy build recipe (Windows, MSBuild 17,
  NUnit console). Mind the database warning there before running anything.

## Provenance and license

The baseline is the public
[MarlabsInc/SocialGoal](https://github.com/MarlabsInc/SocialGoal) reference
application (last upstream commit June 2014), preserved unmodified at the
`legacy-baseline` tag, original README included. MIT licensed
([LICENSE.md](LICENSE.md)); attribution to Marlabs and the original team.

Evincia is not affiliated with Marlabs. This is independent analysis and
modernization of publicly available code, undertaken as a
[public-codebase teardown](https://www.evincia.co/teardowns.html) methodology
demonstration.
