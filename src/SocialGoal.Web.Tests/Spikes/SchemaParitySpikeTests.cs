using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Spikes;

/// <summary>
/// Sprint 5 gating spike (epic: "EF Core mapping spike"). Creates one LocalDB
/// database from the schema baseline of record (docs/schema/schema-baseline.sql,
/// the EF6-generated DDL) and a second from the EF Core model's EnsureCreated,
/// then compares the live catalogs for the spike tables: Goal + lookups,
/// AspNetUsers, and FollowUsers (the gnarly relationship -- four FKs to
/// AspNetUsers, two via EF6 shadow columns). Passing means the EF Core mapping
/// approach reproduces the legacy schema exactly for this slice, which is the
/// evidence the Sprint 6-7 rebuild budget is gated on.
/// </summary>
[TestFixture]
[NonParallelizable]
public class SchemaParitySpikeTests
{
    private const string BaselineDb = "SocialGoal_ModernSpike_Baseline";
    private const string EfCoreDb = "SocialGoal_ModernSpike_EfCore";

    private static readonly string[] SpikeTables =
        ["AspNetUsers", "FollowUsers", "Goals", "GoalStatus", "Metrics"];

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
        // EnsureCreated on an existing empty database creates the model's
        // tables only -- exactly the spike subset.
        context.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public void DropBothDatabases()
    {
        LocalDb.DropDatabase(BaselineDb);
        LocalDb.DropDatabase(EfCoreDb);
    }

    [Test]
    public void Columns_MatchBaselineExactly()
    {
        var baseline = SqlCatalog.ReadColumns(BaselineDb, SpikeTables);
        var efCore = SqlCatalog.ReadColumns(EfCoreDb, SpikeTables);

        // Guards against a vacuous pass: 13 AspNetUsers + 7 FollowUsers
        // (incl. both shadow columns) + 11 Goals + 2 GoalStatus + 2 Metrics.
        Assert.That(baseline, Has.Count.EqualTo(35),
            "Baseline catalog read must see every spike-table column.");
        Assert.That(efCore, Is.EqualTo(baseline),
            "EF Core-generated columns (name, type, length, nullability, identity) must match the EF6 baseline.");
    }

    [Test]
    public void PrimaryKeyColumns_MatchBaseline()
    {
        var baseline = SqlCatalog.ReadPrimaryKeys(BaselineDb, SpikeTables);
        var efCore = SqlCatalog.ReadPrimaryKeys(EfCoreDb, SpikeTables);

        // PK constraint *names* are excluded: the EF6 baseline left them
        // system-generated; EF Core names them PK_<table>. Immaterial to the
        // app and to the Sprint 6-7 migration.
        Assert.That(efCore, Is.EqualTo(baseline),
            "Primary-key column sets must match the baseline.");
    }

    [Test]
    public void ForeignKeys_MatchBaseline_IncludingShadowColumnsAndCascades()
    {
        var baseline = SqlCatalog.ReadForeignKeys(BaselineDb, SpikeTables);
        var efCore = SqlCatalog.ReadForeignKeys(EfCoreDb, SpikeTables);

        // 3 on Goals + 4 on FollowUsers; anything less means the reader or
        // the spike-table filter went wrong, not that parity holds.
        Assert.That(baseline, Has.Count.EqualTo(7),
            "Baseline catalog read must see all seven spike-table FKs.");
        Assert.That(efCore, Is.EqualTo(baseline),
            "FK names, columns (including EF6's ApplicationUser_Id/_Id1 shadow columns), targets, and delete actions must match.");
    }

    [Test]
    public void IndexDelta_IsExactlyTheDocumentedEfCoreAdditions()
    {
        var baseline = SqlCatalog.ReadNonPrimaryKeyIndexes(BaselineDb, SpikeTables);
        var efCore = SqlCatalog.ReadNonPrimaryKeyIndexes(EfCoreDb, SpikeTables);

        // Known, intentional divergence: the EF6 baseline created no FK
        // indexes; EF Core creates one per FK column. These are additive and
        // desirable, and go to Sprints 6-7 as documented additions (per the
        // exit-gate wording "no schema drift except intentional, documented
        // additions"). This test pins the delta so it cannot grow silently.
        Assert.That(baseline, Is.Empty, "The EF6 baseline has no non-PK indexes on the spike tables.");
        Assert.That(efCore, Is.EqualTo(new[]
        {
            "FollowUsers: IX_FollowUsers_ApplicationUser_Id",
            "FollowUsers: IX_FollowUsers_ApplicationUser_Id1",
            "FollowUsers: IX_FollowUsers_FromUserId",
            "FollowUsers: IX_FollowUsers_ToUserId",
            "Goals: IX_Goals_GoalStatusId",
            "Goals: IX_Goals_MetricId",
            "Goals: IX_Goals_UserId",
        }), "EF Core's FK-index additions -- the only allowed index delta.");
    }
}
