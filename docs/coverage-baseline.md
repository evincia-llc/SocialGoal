# Coverage baseline (Sprint 2, 2026-07-24)

Measured with OpenCover + NUnit 2.6 console over the Release build, scoped to
solution assemblies (`+[SocialGoal*]* -[SocialGoal.Tests]*`), 144 tests.
Reports are CI artifacts (`coverage-report` on each `legacy-ci` run) -- never
committed; this file records the numbers.

**Overall: 50.7% line / 26.8% branch** (2628/5175 lines).
LMRR baseline for comparison (pre-characterization, 113 tests): 44.5% line /
23.8% branch -- the delta is the Sprint 2 data-layer suite.

| Assembly | Line % | Covered / Coverable | Note |
|---|---|---|---|
| SocialGoal (Web) | 46.3 | 1236 / 2666 | controller fixtures |
| SocialGoal.Core | 0 | 0 / 14 | one file, unused enum plumbing |
| SocialGoal.Data | **72** | 309 / 429 | was 0% -- R-007 dark seam lit |
| SocialGoal.Model | 86.3 | 209 / 242 | POCOs; high but trivial |
| SocialGoal.Service | 50.9 | 787 / 1546 | exercised via controller tests |
| SocialGoal.Web.Core | 31.2 | 87 / 278 | filters/helpers; the two inert security filters remain uncovered |

Reading notes (per the measurement method's own caveats): Model's 86% is
instantiation coverage, not behavior; branch well under line = happy-path
testing; Web.Core's uncovered filters are the R-007 auth/anti-forgery seam --
that darkness is Sprint 3's target, not this sprint's.

CI enforcement: `legacy-ci` fails if `SocialGoal.Data` line coverage returns
to 0 (Sprint 2 exit-gate floor). The per-run summary lands in the workflow
step summary; the HTML report is the `coverage-report` artifact.
