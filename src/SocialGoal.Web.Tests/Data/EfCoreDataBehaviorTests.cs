using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Data;

/// <summary>
/// The EF Core port of the Sprint 2 data-layer characterization suite
/// (source/SocialGoal.Tests/Data/RepositoryBaseCharacterizationTests.cs).
/// Same discipline: these PIN observed behavior, defects included; they are not
/// a specification of desired behavior. The repository/UnitOfWork layer they
/// originally exercised is retired, but the *semantics* they pinned are exactly
/// what Sprint 7's service rewrite inherits, so they are re-pinned here at the
/// DbContext level against a real LocalDB.
///
/// Where EF Core's behavior differs from the EF6 baseline the test says so in
/// the form LEGACY ... / EF CORE ..., because the epic requires behavior
/// differences to be reconciled deliberately rather than discovered later.
///
/// Legacy tests deliberately not ported here:
/// * GetPage / PagedList (3 tests) -- paging is Sprint 7's query layer, and
///   PagedList does not exist on this side.
/// * Get(predicate) first-match / null (2 tests) -- their EF Core equivalent is
///   FirstOrDefault, i.e. framework behavior with no application semantics left
///   to pin.
/// * DatabaseFactory_Get_ReturnsSameContextInstance -- the modern analogue is
///   DI scope lifetime, which is a host-composition concern, not a DbContext
///   one. The hazard it guarded is pinned by the write-loss test below.
/// </summary>
[TestFixture]
[NonParallelizable]
public class EfCoreDataBehaviorTests
{
    private const string BehaviorDb = "SocialGoal_ModernBehavior";

    /// <summary>
    /// Fixed instants, never DateTime.Now: the modernization rules forbid it,
    /// and a fixed value keeps the assertions deterministic.
    /// </summary>
    private static readonly DateTime SeedDate = new(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime EditDate = new(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc);

    [OneTimeSetUp]
    public void CreateDatabase()
    {
        LocalDb.CreateDatabase(BehaviorDb);
        using var context = NewContext();
        context.Database.Migrate();
    }

    [OneTimeTearDown]
    public void DropDatabase() => LocalDb.DropDatabase(BehaviorDb);

    // Each test uses its own marker values rather than truncating shared
    // tables, so the fixture never has to fight the migration's seed rows and
    // the tests stay order-independent.
    private static SocialGoalDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SocialGoalDbContext>()
            .UseSqlServer(LocalDb.ConnectionStringFor(BehaviorDb))
            .Options);

    private static int SeedMetric(string type)
    {
        using var context = NewContext();
        var metric = new Metric { Type = type };
        context.Metrics.Add(metric);
        context.SaveChanges();
        return metric.MetricId;
    }

    private static int CountMetrics(string type)
    {
        using var context = NewContext();
        return context.Metrics.Count(m => m.Type == type);
    }

    private static int SeedUserProfile()
    {
        using var context = NewContext();
        var profile = new UserProfile
        {
            DateEdited = SeedDate,
            Email = "original@example.test",
            FirstName = "Original",
            City = "Bristol",
            Country = "UK",
        };
        context.UserProfiles.Add(profile);
        context.SaveChanges();
        return profile.UserProfileId;
    }

    // ---- Persistence and the unit of work --------------------------------

    [Test]
    public void Add_WithoutSaveChanges_DoesNotPersist()
    {
        using (var context = NewContext())
        {
            context.Metrics.Add(new Metric { Type = "no-save" });
            // No SaveChanges. Disposing the context discards the tracked Added
            // entity. (LEGACY: the same, via UnitOfWork.Commit never called.)
        }

        Assert.That(CountMetrics("no-save"), Is.Zero);
    }

    [Test]
    public void AddThenSaveChanges_Persists()
    {
        using (var context = NewContext())
        {
            context.Metrics.Add(new Metric { Type = "saved" });
            context.SaveChanges();
        }

        Assert.That(CountMetrics("saved"), Is.EqualTo(1));
    }

    [Test]
    public void AddOnOneContext_SaveChangesOnAnother_SilentlyLosesTheWrite()
    {
        // The hazard Sprint 2 pinned on the legacy side, unchanged here: an
        // entity Added to context A is invisible to context B, so B's
        // SaveChanges writes nothing AND reports nothing. No exception, no
        // return-value signal beyond a zero.
        //
        // LEGACY: two DatabaseFactory instances meant two DbContexts -- the
        // repository added to one while the UnitOfWork committed the other.
        // EF CORE: the same silence, reachable the same way. Which is why
        // Sprint 7's services must take the scoped context by injection and
        // never new one up mid-operation.
        int saved;
        using (var writeContext = NewContext())
        using (var saveContext = NewContext())
        {
            writeContext.Metrics.Add(new Metric { Type = "orphaned" });
            saved = saveContext.SaveChanges();
        }

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Zero, "The saving context had nothing tracked, and said so only by returning 0.");
            Assert.That(CountMetrics("orphaned"), Is.Zero, "The write is gone, silently.");
        });
    }

    // ---- Identity map ------------------------------------------------------

    [Test]
    public void SameContext_ReturnsTheSameInstanceForTheSameKey()
    {
        var id = SeedMetric("identity-map");

        using var context = NewContext();
        var first = context.Metrics.Single(m => m.MetricId == id);
        var second = context.Metrics.Single(m => m.MetricId == id);

        // The second query still hits the database, but materialization is
        // short-circuited by the change tracker. (LEGACY: GetById twice on one
        // repository returned the same instance for the same reason.)
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void SeparateContexts_ReturnDifferentInstancesForTheSameKey()
    {
        var id = SeedMetric("identity-map-across");

        using var first = NewContext();
        using var second = NewContext();

        var fromFirst = first.Metrics.Single(m => m.MetricId == id);
        var fromSecond = second.Metrics.Single(m => m.MetricId == id);

        // Each context has its own identity map. Entities do not travel between
        // them, and an entity from one is *detached* as far as the other is
        // concerned -- the precondition for the overwrite hazard below.
        Assert.That(fromSecond, Is.Not.SameAs(fromFirst));
    }

    // ---- Update: the whole-entity overwrite hazard --------------------------

    [Test]
    public void Update_OnADetachedCopy_OverwritesColumnsTheCallerNeverSet()
    {
        var id = SeedUserProfile();

        using (var context = NewContext())
        {
            // A detached instance carrying the key and one edited field -- the
            // exact shape a controller gets from model binding a partial form.
            // Update() marks EVERY property Modified, so the properties the
            // caller never touched are written as nulls over live data.
            //
            // LEGACY: RepositoryBase.Update attached and set the whole entity
            // to EntityState.Modified, with the same result (pinned in
            // Update_DetachedEntity_MarksWholeEntityModified_OverwritesUnsetColumns).
            // EF CORE: identical semantics if Update() is used naively. This is
            // why the modernization rules mandate loading the entity and
            // applying targeted changes instead -- see the contrast test below.
            context.Update(new UserProfile
            {
                UserProfileId = id,
                FirstName = "Edited",
                DateEdited = EditDate,
            });
            context.SaveChanges();
        }

        using var verify = NewContext();
        var stored = verify.UserProfiles.Single(p => p.UserProfileId == id);

        Assert.Multiple(() =>
        {
            Assert.That(stored.FirstName, Is.EqualTo("Edited"), "The intended edit landed.");
            Assert.That(stored.City, Is.Null, "...and so did an unintended null over 'Bristol'.");
            Assert.That(stored.Country, Is.Null, "...and over 'UK'.");
            Assert.That(stored.Email, Is.Null, "...and over the address the user had on file.");
        });
    }

    [Test]
    public void EditingATrackedEntity_MarksOnlyTheChangedPropertyModified()
    {
        var id = SeedUserProfile();

        using (var context = NewContext())
        {
            var profile = context.UserProfiles.Single(p => p.UserProfileId == id);
            profile.FirstName = "Edited";

            var entry = context.Entry(profile);
            Assert.Multiple(() =>
            {
                Assert.That(entry.Property(p => p.FirstName).IsModified, Is.True);
                Assert.That(entry.Property(p => p.City).IsModified, Is.False,
                    "Load-then-edit produces a targeted UPDATE; this is the pattern Sprint 7's services must use.");
            });

            context.SaveChanges();
        }

        using var verify = NewContext();
        var stored = verify.UserProfiles.Single(p => p.UserProfileId == id);

        Assert.Multiple(() =>
        {
            Assert.That(stored.FirstName, Is.EqualTo("Edited"));
            Assert.That(stored.City, Is.EqualTo("Bristol"), "Untouched columns survive, unlike the Update() path above.");
        });
    }

    // ---- Materialization ---------------------------------------------------

    [Test]
    public void ToList_IsAMaterializedSnapshot_UnaffectedByLaterInserts()
    {
        SeedMetric("snapshot");
        SeedMetric("snapshot");

        using var context = NewContext();
        var materialized = context.Metrics.Where(m => m.Type == "snapshot").ToList();
        Assert.That(materialized, Has.Count.EqualTo(2));

        SeedMetric("snapshot");

        // (LEGACY: GetAll/GetMany returned List<T> with the same consequence.)
        Assert.That(materialized, Has.Count.EqualTo(2),
            "A materialized list is a snapshot; a row inserted afterwards is not in it.");
    }

    // ---- Bulk delete: a real delta from the legacy repository ---------------

    [Test]
    public void ExecuteDelete_DeletesMatchingRowsWithoutLoadingThem_AndLeavesTheTrackerStale()
    {
        var firstId = SeedMetric("bulk-del");
        SeedMetric("bulk-del");
        SeedMetric("bulk-keep");

        using var context = NewContext();
        var tracked = context.Metrics.Single(m => m.MetricId == firstId);

        var deleted = context.Metrics.Where(m => m.Type == "bulk-del").ExecuteDelete();

        Assert.Multiple(() =>
        {
            // DELTA. LEGACY: RepositoryBase.Delete(predicate) materialized every
            // match and removed them one at a time through the change tracker,
            // and nothing happened until UnitOfWork.Commit.
            // EF CORE: ExecuteDelete issues a single DELETE immediately -- no
            // materialization, no SaveChanges, and the change tracker is never
            // told. Faster and far less chatty, but a tracked entity whose row
            // has just been deleted still reports Unchanged, and a later
            // SaveChanges on that context can throw a concurrency exception.
            // Sprint 7 gets the performance; it also gets this caveat.
            Assert.That(deleted, Is.EqualTo(2), "ExecuteDelete returns the row count, immediately.");
            Assert.That(context.Entry(tracked).State, Is.EqualTo(EntityState.Unchanged),
                "The tracker still believes a row that no longer exists is present and clean.");
            Assert.That(CountMetrics("bulk-del"), Is.Zero);
            Assert.That(CountMetrics("bulk-keep"), Is.EqualTo(1));
        });
    }

    [Test]
    public void ExecuteDelete_WithNoMatchingRows_IsANoOp()
    {
        SeedMetric("bulk-survivor");

        using var context = NewContext();
        var deleted = context.Metrics.Where(m => m.Type == "does-not-exist").ExecuteDelete();

        Assert.Multiple(() =>
        {
            Assert.That(deleted, Is.Zero);
            Assert.That(CountMetrics("bulk-survivor"), Is.EqualTo(1));
        });
    }

    // ---- Lifecycle ---------------------------------------------------------

    [Test]
    public void UsingADisposedContext_ThrowsObjectDisposedException()
    {
        var context = NewContext();
        context.Dispose();

        // DELTA. LEGACY: EF6 threw InvalidOperationException from a disposed
        // DbContext (pinned in DatabaseFactory_AfterDispose_ContextIsDisposed).
        // EF CORE: ObjectDisposedException. Any Sprint 7 code that catches the
        // legacy type to detect a dead context would silently stop catching.
        Assert.Throws<ObjectDisposedException>(() => context.Metrics.ToList());
    }

    // ---- Consequence of dropping the legacy constructor defaults ------------

    [Test]
    public void ADateTimeLeftAtItsDefault_DoesNotFitTheDatetimeColumns()
    {
        // The legacy entities set their date columns from DateTime.Now in the
        // constructor. The port deliberately dropped those defaults (plain
        // POCOs; no DateTime.Now anywhere per the modernization rules), which
        // moves the obligation to Sprint 7's services -- and this is what
        // happens if one of them forgets: default(DateTime) is 0001-01-01,
        // outside SQL Server's `datetime` range, which starts at 1753-01-01.
        // It fails loudly at SaveChanges rather than storing a wrong date.
        using var context = NewContext();
        context.UserProfiles.Add(new UserProfile { FirstName = "no-date" });

        var ex = Assert.Throws<DbUpdateException>(() => context.SaveChanges());

        // Pin the cause, not just the wrapper: a bare DbUpdateException assert
        // would keep passing if the row started failing for some unrelated
        // reason, and the claim above would quietly stop being true.
        Assert.That(ex!.InnerException?.Message, Does.StartWith("SqlDateTime overflow"),
            "Expected the 1753 lower bound of SQL Server's `datetime` to be what rejects default(DateTime).");
    }
}
