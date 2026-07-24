# Schema baseline of record (Sprint 2)

`schema-baseline.sql` is the DDL the EF6 model actually generates
(`ObjectContext.CreateDatabaseScript()` from `SocialGoalEntities`): 30 tables,
28 foreign keys, all `dbo`. It is the baseline schema of record for the EF Core
migration (epic Sprints 6-7): the EF Core model must reproduce this schema, and
any intentional divergence needs a decision recorded in
`ai-context/decisions.md` first.

Guarded by two always-on tests in `SocialGoal.Tests/Data/SchemaSnapshotTests.cs`:

- `SchemaBaseline_MatchesGeneratedModelDdl` -- regenerates the script from the
  live model each CI run and fails on any drift from the committed file.
- `SchemaBaseline_ChecksumMatches` -- verifies `schema-baseline.sha256`
  (SHA-256 over LF-normalized UTF-8 of the .sql file).

Regenerating the baseline: delete `schema-baseline.sql`, run the
`SchemaSnapshotTests` fixture (it re-emits the file and fails once as a
safety), review the diff, update the checksum, and commit -- together with the
decision that justified the schema change.

Facts worth knowing (pinned by the mapping smoke tests):

- The physical model comes from the *generated* model, not the configuration
  source: `ApplicationUserConfiguration` exists but is never registered, so
  none of its settings apply; `GoalUpdateConfiguration` is fully orphaned
  (unregistered, no DbSet -- the `GoalUpdate` entity is absent from the model);
  `RegistrationToken` has no DbSet yet IS in the model because its
  configuration is registered.
- EF pluralization quirks: `Focus -> Foci`, `GoalStatus -> GoalStatus`.
- A live database also carries EF's `__MigrationHistory` table (added by
  `Database.Create()`, not part of the model DDL).

`triggers.md` closes the LMRR's open unknown on database triggers.
