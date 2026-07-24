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

## Sprint 3 update (2026-07-24)

Measured the same way CI's floor check does -- OpenCover over the Release build,
line % via ReportGenerator 5.4.1 `JsonSummary` (`legacy-ci.yml` step "Render
coverage summary") -- now over **187 tests** (144 + 16 enforcement-surface + 27
behavioral matrix). The behavioral matrix invokes the real Web controllers and
their service graph against LocalDB, so it lifts the enforcement-path assemblies.

**Overall: ~55% line** (2084/3781). Per assembly, with the Sprint 2 line % for
comparison:

| Assembly | S2 line % | S3 line % | Note |
|---|---|---|---|
| SocialGoal (Web) | 46.3 | **48.2** | controllers now exercised behaviorally, not only via mocked fixtures |
| SocialGoal.Core | 0 | 0 | unused enum plumbing |
| SocialGoal.Data | 72 | **87.6** | well above the CI 0% floor |
| SocialGoal.Model | 86.3 | 88.2 | POCO instantiation |
| SocialGoal.Service | 50.9 | **55.0** | real service execution under the matrix |
| SocialGoal.Web.Core | 31.2 | **35.9** | helpers/extensions used by the controllers |

Method note: the Sprint 2 *table* above reported raw OpenCover sequence points;
this Sprint 3 table reports ReportGenerator line % (CI's own metric). Absolute
coverable counts drift between OpenCover runs, so read cross-sprint deltas as
directional, not exact. ReportGenerator line % is the method of record going
forward.

The R-007 auth/anti-forgery seam -- Sprint 3's named target -- is now **lit by
characterization, not by coverage of the filters themselves**. The two custom
filters (`SocialGoalAuthorizeAttribute`, `AntiForgeryTokenFilterProvider`) stay
in Web.Core's uncovered remainder *by construction*: they are unregistered dead
code, so they are unreachable and cannot be executed. Their darkness is no longer
an unknown -- it is proven-dead, pinned by `InertFilterCharacterizationTests`,
and the actual enforcement surface is pinned by the 187-test authorization suite
(`docs/security/authorization-matrix.md`). Covering dead filters would mean
wiring them, which is a Phase 2 decision, not a Phase 0 characterization.
