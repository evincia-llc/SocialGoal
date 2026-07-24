using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Data;

/// <summary>
/// Sprint 6 full-schema parity. Creates one LocalDB database from the schema
/// baseline of record (docs/schema/schema-baseline.sql, the EF6-generated DDL)
/// and a second from the EF Core model, then compares the *entire* dbo catalog:
/// tables, column facets, primary keys, foreign keys and indexes. Nothing is
/// filtered out, so a table or constraint on one side only fails the run.
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
        using var context = new SocialGoalDbContext(
            new DbContextOptionsBuilder<SocialGoalDbContext>()
                .UseSqlServer(LocalDb.ConnectionStringFor(EfCoreDb))
                .Options);
        // EnsureCreated, not Migrate: the baseline migration is a later Sprint 6
        // work unit, at which point this fixture switches to Database.Migrate()
        // and picks up __EFMigrationsHistory as a known, pinned extra table.
        context.Database.EnsureCreated();

        BaselineTables.UnionWith(SqlCatalog.ReadTableNames(BaselineDb));
    }

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
        Assert.That(efCore, Is.EqualTo(BaselineTables),
            "The EF Core model must create every baseline table and no others -- "
            + "including no __EFMigrationsHistory, which EnsureCreated does not write.");
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
        // indexes; EF Core creates one per FK column that is not already
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
