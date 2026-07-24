using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Data;

/// <summary>
/// Sprint 6 full-schema parity. Creates one LocalDB database from the schema
/// baseline of record (docs/schema/schema-baseline.sql, the EF6-generated DDL)
/// and a second by applying the EF Core <c>Baseline</c> migration, then
/// compares the *entire* dbo catalog: tables, column facets, primary keys,
/// foreign keys and indexes. Nothing is filtered out beyond the single
/// allowlisted migrations-history table, so a table or constraint on one side
/// only fails the run.
///
/// The database under test is built by <c>Database.Migrate()</c> (D17.3), which
/// makes this the check that the *migration* -- the thing that will actually be
/// run against a real database -- reproduces the legacy schema. Model-to-
/// migration drift is caught separately by MigrationModelDriftTests.
///
/// This supersedes the Sprint 5 SchemaParitySpikeTests, which proved the same
/// properties over a five-table subset -- every assertion it made is made here
/// over the full 30-table catalog.
/// </summary>
[TestFixture]
[NonParallelizable]
public class SchemaParityTests
{
    private const string BaselineDb = "SocialGoal_ModernParity_Baseline";
    private const string EfCoreDb = "SocialGoal_ModernParity_EfCore";

    /// <summary>
    /// The one table the migration path adds and the baseline cannot have:
    /// EF Core's applied-migrations ledger, the analogue of EF6's
    /// __MigrationHistory, which lives outside the model. Anything else
    /// appearing on either side is a failure -- the allowlist is exactly one
    /// name and is asserted to have been used.
    /// </summary>
    private const string MigrationsHistoryTable = "__EFMigrationsHistory";

    /// <summary>
    /// Every dbo table in the baseline, read once in setup and reused as the
    /// filter for the per-object catalog reads.
    /// </summary>
    private static readonly SortedSet<string> BaselineTables = new(StringComparer.Ordinal);

    [OneTimeSetUp]
    public void CreateBothDatabases()
    {
        LocalDb.CreateDatabase(BaselineDb);
        LocalDb.ExecuteScript(BaselineDb, File.ReadAllText(LocalDb.SchemaBaselinePath()));

        LocalDb.CreateDatabase(EfCoreDb);
        using var context = CreateContext();
        context.Database.Migrate();

        BaselineTables.UnionWith(SqlCatalog.ReadTableNames(BaselineDb));
    }

    private static SocialGoalDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<SocialGoalDbContext>()
            .UseSqlServer(LocalDb.ConnectionStringFor(EfCoreDb))
            .Options);

    [OneTimeTearDown]
    public void DropBothDatabases()
    {
        LocalDb.DropDatabase(BaselineDb);
        LocalDb.DropDatabase(EfCoreDb);
    }

    [Test]
    public void Tables_MatchBaselineExactly()
    {
        var efCore = SqlCatalog.ReadTableNames(EfCoreDb);

        Assert.That(BaselineTables, Has.Count.EqualTo(30),
            "The baseline of record has 30 tables; a different count means the baseline or the reader changed.");
        Assert.That(efCore, Does.Contain(MigrationsHistoryTable),
            "Migrate() must have written the migrations-history table; if it is absent the allowlist below is hiding nothing "
            + "and the comparison is not testing what it claims to.");

        efCore.Remove(MigrationsHistoryTable);
        Assert.That(efCore, Is.EqualTo(BaselineTables),
            "Once the migrations-history table is set aside, the migrated database must hold every baseline table and no others.");
    }

    [Test]
    public void SeedData_IsTheLegacyReferenceData()
    {
        using var context = CreateContext();

        var metrics = context.Metrics
            .OrderBy(m => m.MetricId)
            .Select(m => new { m.MetricId, m.Type })
            .ToList();
        var statuses = context.GoalStatuses
            .OrderBy(s => s.GoalStatusId)
            .Select(s => new { s.GoalStatusId, s.GoalStatusType })
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(
                metrics.Select(m => $"{m.MetricId}:{m.Type}"),
                Is.EqualTo(new[]
                {
                    "1:%", "2:$", "3:$ M", "4:Rs", "5:Hours", "6:Km", "7:Kg", "8:Years",
                }),
                "Metrics seed: the legacy values, with the ids legacy left to identity ordering now stated explicitly.");

            // GoalStatusId 1 must be "In Progress": the legacy Goal and
            // GroupGoal constructors hardcode GoalStatusId = 1, so a
            // re-ordering here would silently change what every new goal means.
            Assert.That(
                statuses.Select(s => $"{s.GoalStatusId}:{s.GoalStatusType}"),
                Is.EqualTo(new[] { "1:In Progress", "2:On Hold", "3:Completed" }),
                "GoalStatus seed, id 1 = In Progress.");
        });
    }

    [Test]
    public void Columns_MatchBaselineExactly()
    {
        var baseline = SqlCatalog.ReadColumns(BaselineDb, BaselineTables);
        var efCore = SqlCatalog.ReadColumns(EfCoreDb, BaselineTables);

        // Guards against a vacuous pass: the baseline's 30 tables carry 153
        // columns in total (counted from the baseline catalog read).
        Assert.That(baseline, Has.Count.EqualTo(153),
            "Baseline catalog read must see every column in the schema of record.");
        Assert.That(efCore, Is.EqualTo(baseline),
            "EF Core-generated columns (name, type, length, nullability, identity) must match the EF6 baseline.");
    }

    [Test]
    public void PrimaryKeyColumns_MatchBaseline()
    {
        var baseline = SqlCatalog.ReadPrimaryKeys(BaselineDb, BaselineTables);
        var efCore = SqlCatalog.ReadPrimaryKeys(EfCoreDb, BaselineTables);

        // PK constraint *names* are excluded: the EF6 baseline left them
        // system-generated; EF Core names them PK_<table>. Immaterial to the
        // app and to the migration. Column order is not -- the two composite
        // keys (AspNetUserLogins, AspNetUserRoles) are compared ordinally.
        Assert.That(baseline, Has.Count.EqualTo(30), "Every baseline table has a primary key.");
        Assert.That(efCore, Is.EqualTo(baseline),
            "Primary-key column sets and their order must match the baseline.");
    }

    [Test]
    public void ForeignKeys_MatchBaseline_IncludingShadowColumnsAndCascades()
    {
        var baseline = SqlCatalog.ReadForeignKeys(BaselineDb, BaselineTables);
        var efCore = SqlCatalog.ReadForeignKeys(EfCoreDb, BaselineTables);

        // 28 and not one more: eleven entities hold bare user-id columns that
        // EF6 never constrained (no navigation on the legacy class), and so
        // does GroupUsers.GroupId, GroupGoals.AssignedGroupUserId and
        // GroupUpdateSupports.GroupUserId. Inventing FKs for them would be a
        // schema change, so the count is pinned in both directions.
        Assert.That(baseline, Has.Count.EqualTo(28),
            "Baseline catalog read must see all 28 FKs in the schema of record.");
        Assert.That(efCore, Is.EqualTo(baseline),
            "FK names, columns (including EF6's ApplicationUser_Id/_Id1 shadow columns), targets, and delete actions must match.");
    }

    [Test]
    public void IndexDelta_IsExactlyTheDocumentedEfCoreAdditions()
    {
        var baseline = SqlCatalog.ReadNonPrimaryKeyIndexes(BaselineDb, BaselineTables);
        var efCore = SqlCatalog.ReadNonPrimaryKeyIndexes(EfCoreDb, BaselineTables);

        // Known, intentional divergence: the EF6 baseline created no FK
        // indexes; the migration creates one per FK column that is not already
        // covered by an existing index (which is why AspNetUserLogins.UserId
        // and AspNetUserRoles.UserId -- both leading PK columns -- are absent).
        // These are additive and desirable, and go on the record as the epic's
        // documented schema additions. This test pins the delta so it cannot
        // grow silently.
        Assert.That(baseline, Is.Empty, "The EF6 baseline has no non-PK indexes.");
        Assert.That(efCore, Is.EqualTo(new[]
        {
            "AspNetUserClaims: IX_AspNetUserClaims_User_Id",
            "AspNetUserRoles: IX_AspNetUserRoles_RoleId",
            "Comments: IX_Comments_UpdateId",
            "Foci: IX_Foci_GroupId",
            "FollowUsers: IX_FollowUsers_ApplicationUser_Id",
            "FollowUsers: IX_FollowUsers_ApplicationUser_Id1",
            "FollowUsers: IX_FollowUsers_FromUserId",
            "FollowUsers: IX_FollowUsers_ToUserId",
            "Goals: IX_Goals_GoalStatusId",
            "Goals: IX_Goals_MetricId",
            "Goals: IX_Goals_UserId",
            "GroupComments: IX_GroupComments_GroupUpdateId",
            "GroupGoals: IX_GroupGoals_FocusId",
            "GroupGoals: IX_GroupGoals_GoalStatusId",
            "GroupGoals: IX_GroupGoals_GroupId",
            "GroupGoals: IX_GroupGoals_GroupUserId",
            "GroupGoals: IX_GroupGoals_MetricId",
            "GroupInvitations: IX_GroupInvitations_GroupId",
            "GroupRequests: IX_GroupRequests_GroupId",
            "GroupRequests: IX_GroupRequests_UserId",
            "GroupUpdateSupports: IX_GroupUpdateSupports_GroupUpdateId",
            "GroupUpdates: IX_GroupUpdates_GroupGoalId",
            "SupportInvitations: IX_SupportInvitations_GoalId",
            "Supports: IX_Supports_GoalId",
            "UpdateSupports: IX_UpdateSupports_UpdateId",
            "Updates: IX_Updates_GoalId",
        }), "EF Core's FK-index additions -- the only allowed index delta.");
    }
}
