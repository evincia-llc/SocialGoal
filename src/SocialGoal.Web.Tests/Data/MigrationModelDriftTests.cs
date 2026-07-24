using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using SocialGoal.Web.Data;

namespace SocialGoal.Web.Tests.Data;

/// <summary>
/// The in-suite equivalent of <c>dotnet ef migrations has-pending-model-changes</c>.
/// It exists as a test rather than a CI step because modern-ci runs tests, not
/// ef commands: a mapping change that nobody scaffolds a migration for is
/// exactly the drift that would otherwise reach a database as a surprise.
///
/// This runs the same comparison the ef command does -- the last migration's
/// model snapshot against the current model, through the migrations model
/// differ -- and touches no database.
/// </summary>
[TestFixture]
public class MigrationModelDriftTests
{
    [Test]
    public void Migrations_HaveNoPendingModelChanges()
    {
        // Never opened: the differ works entirely off metadata. A provider is
        // required only so the relational model can be built.
        using var context = new SocialGoalDbContext(
            new DbContextOptionsBuilder<SocialGoalDbContext>()
                .UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SocialGoal_NeverOpened")
                .Options);

        // EF1001: the model differ and the migrations assembly are EF-internal
        // APIs. They are what the ef tooling itself calls for this exact check,
        // and there is no public equivalent; the alternative is not running the
        // check in CI at all.
#pragma warning disable EF1001
        var snapshot = context.GetService<IMigrationsAssembly>().ModelSnapshot;
        Assert.That(snapshot, Is.Not.Null,
            "No model snapshot found. The Baseline migration and its SocialGoalDbContextModelSnapshot "
            + "must be compiled into the web assembly, or this test passes vacuously.");

        var snapshotModel = snapshot!.Model;
        if (snapshotModel is IMutableModel mutableModel)
        {
            snapshotModel = mutableModel.FinalizeModel();
        }

        // designTime: true, and it matters. The right-hand side of the diff is
        // IDesignTimeModel.Model, so the snapshot has to be finalized the same
        // way or the two models disagree about provider annotations that were
        // never in dispute: initializing it as a runtime model reports a
        // spurious AlterColumn for every identity primary key in the schema
        // (verified against `dotnet ef migrations has-pending-model-changes`,
        // which reported no changes for the same pair).
        snapshotModel = context.GetService<IModelRuntimeInitializer>()
            .Initialize(snapshotModel, designTime: true);

        var differences = context.GetService<IMigrationsModelDiffer>().GetDifferences(
            snapshotModel.GetRelationalModel(),
            context.GetService<IDesignTimeModel>().Model.GetRelationalModel());
#pragma warning restore EF1001

        Assert.That(differences.Select(Describe), Is.Empty,
            "The entity model has changed since the last migration was scaffolded. "
            + "Run `dotnet ef migrations add <Name> --project src/SocialGoal.Web --output-dir Data/Migrations` "
            + "and review the generated DDL against docs/schema/schema-baseline.sql before committing.");
    }

    private static string Describe(MigrationOperation operation) => operation switch
    {
        AlterColumnOperation alter =>
            $"AlterColumn {alter.Table}.{alter.Name}: {alter.OldColumn.ColumnType} null={alter.OldColumn.IsNullable} "
            + $"-> {alter.ColumnType} null={alter.IsNullable}",
        _ => operation.GetType().Name,
    };
}
