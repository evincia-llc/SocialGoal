---
name: characterization-tests
description: Write characterization tests for SocialGoal legacy behavior using the proven LocalDB harness -- pin what the code actually does (including defects), never fix. Use when adding pinning coverage in Phase 0-1 or porting the suite for EF Core parity in Sprints 6-7.
---

# Characterization tests (proven pattern, Sprint 2)

Reference implementation: `source/SocialGoal.Tests/Data/` -- copy its shape.

## Harness (LocalDB, explicit lifecycle)

* One `[SetUpFixture]` per test namespace (`CharacterizationDatabase.cs`):
  `Database.SetInitializer<SocialGoalEntities>(null)` first -- we own the
  lifecycle, no framework initializer ever runs -- then explicit
  `Database.Delete()` + `Database.Create()` in SetUp, `Delete()` in TearDown.
* The connection string (`App.config`, name `SocialGoalEntities`) targets a
  dedicated LocalDB catalog. Never point it at a database you care about.
* Between-fixture cleanup is FK-safe deletion via `TestDataHelper` (delete in
  dependency order); table names resolve from EF store metadata, not
  hardcoded strings.
* Dispose every context and repository factory you create (`TearDown`), or
  Copilot/CI will rightly flag it.

## Discipline: pin, never fix

* Assert what the code DOES, defects included. Sprint 2 pinned silent write
  loss across separate `DatabaseFactory` instances and blind full-row
  updates -- those are findings, not bugs to patch here. Fixing belongs to
  Phase 2; a "fix" in a characterization test is Phase 2 leaking into Phase 0.
* Pin exact observable behavior: concrete exception types, materialization
  semantics (`GetMany` snapshots unaffected by later inserts), ordering,
  paging boundaries.
* Name tests as behavior statements
  (`Commit_OnSecondFactoryInstance_SilentlyLosesFirstWrite`), and comment WHY
  the pinned behavior is surprising, with the register/journal cross-ref.
* A pinned defect gets an `ai-context/lmrr-feedback.md` entry (evidence =
  test name + file) when it confirms/extends/contradicts an LMRR finding.

## CI integration

* `legacy-ci.yml` pre-starts LocalDB on windows-latest (`sqllocaldb`), then
  runs the suite via NUnit console.
* Coverage: OpenCover with `-register:path64` (NOT `-register:user` -- it
  flaked profiler attach), then verify the profiled module actually appears
  in coverage.xml and retry once if not; enforce the data-layer floor.
* Numbers are recorded in `docs/coverage-baseline.md`; reports stay CI
  artifacts, never committed.

## Reuse map

* Sprint 3: same discipline for the authorization/CSRF matrix (HTTP-level
  pinning; flip to enforcement specs during Phase 2 rebuilds).
* Sprints 6-7: this exact suite is the EF Core parity net -- same tests, new
  provider; behavior deltas are deliberate decisions, not silent drift.
