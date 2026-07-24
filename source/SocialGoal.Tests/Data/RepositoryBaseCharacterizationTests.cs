using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PagedList;
using SocialGoal.Data.Infrastructure;
using SocialGoal.Data.Models;
using SocialGoal.Data.Repository;
using SocialGoal.Model.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Characterization tests for RepositoryBase&lt;T&gt;, UnitOfWork and
    /// DatabaseFactory, exercised through MetricRepository against a real
    /// LocalDB. These PIN current behavior; they are not a specification of
    /// desired behavior. Where the observed semantics are hazardous (e.g. the
    /// second-context write-loss below), the test documents the hazard.
    /// </summary>
    [TestFixture]
    public class RepositoryBaseCharacterizationTests
    {
        [SetUp]
        public void CleanSlate()
        {
            using (var context = new SocialGoalEntities())
            {
                TestDataHelper.CleanCoreTables(context);
            }
        }

        private static int MetricCount(string type)
        {
            using (var context = new SocialGoalEntities())
            {
                return context.Metrics.Count(m => m.Type == type);
            }
        }

        // ---- Commit / factory sharing --------------------------------------

        [Test]
        public void Add_WithoutCommit_DoesNotPersist()
        {
            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            repo.Add(new Metric { Type = "no-commit" });
            // No UnitOfWork.Commit(): SaveChanges never called.
            factory.Dispose();

            Assert.AreEqual(0, MetricCount("no-commit"));
        }

        [Test]
        public void AddThenCommit_SharedFactory_Persists()
        {
            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);
            var unitOfWork = new UnitOfWork(factory);

            repo.Add(new Metric { Type = "shared" });
            unitOfWork.Commit();
            factory.Dispose();

            Assert.AreEqual(1, MetricCount("shared"));
        }

        [Test]
        public void AddThenCommit_SeparateFactories_PersistsNothing()
        {
            // The app wires repositories and the UnitOfWork through DI as a shared
            // DatabaseFactory. If they ever receive DIFFERENT factories they get
            // DIFFERENT DbContexts: the repository's Add lands on context A while
            // Commit saves context B, silently losing the write. This pins that
            // hazard.
            var factoryA = new DatabaseFactory();
            var factoryB = new DatabaseFactory();
            var repo = new MetricRepository(factoryA);
            var unitOfWork = new UnitOfWork(factoryB);

            repo.Add(new Metric { Type = "orphaned" });
            unitOfWork.Commit();
            factoryA.Dispose();
            factoryB.Dispose();

            Assert.AreEqual(0, MetricCount("orphaned"));
        }

        // ---- GetById / identity map ----------------------------------------

        [Test]
        public void GetById_RepeatedCalls_ReturnSameInstance_IdentityMap()
        {
            int id;
            using (var seed = new SocialGoalEntities())
            {
                id = TestDataHelper.SeedMetric(seed, "identity").MetricId;
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            var first = repo.GetById((long)id);
            var second = repo.GetById((long)id);

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            factory.Dispose();
        }

        // ---- Update (detached attach, whole-entity Modified) ---------------

        [Test]
        public void Update_DetachedEntity_ChangedValuePersists()
        {
            int id;
            using (var seed = new SocialGoalEntities())
            {
                id = TestDataHelper.SeedMetric(seed, "original").MetricId;
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);
            var unitOfWork = new UnitOfWork(factory);

            // Brand-new, detached instance carrying only the key + a new value.
            repo.Update(new Metric { MetricId = id, Type = "changed" });
            unitOfWork.Commit();
            factory.Dispose();

            using (var verify = new SocialGoalEntities())
            {
                Assert.AreEqual("changed", verify.Metrics.Single(m => m.MetricId == id).Type);
            }
        }

        [Test]
        public void Update_DetachedEntity_MarksWholeEntityModified_OverwritesUnsetColumns()
        {
            int id;
            using (var seed = new SocialGoalEntities())
            {
                id = TestDataHelper.SeedMetric(seed, "has-value").MetricId;
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);
            var unitOfWork = new UnitOfWork(factory);

            // Type left null on the detached instance. Update marks the WHOLE
            // entity Modified, so the null is written over the existing value.
            repo.Update(new Metric { MetricId = id, Type = null });
            unitOfWork.Commit();
            factory.Dispose();

            using (var verify = new SocialGoalEntities())
            {
                Assert.IsNull(verify.Metrics.Single(m => m.MetricId == id).Type);
            }
        }

        // ---- Delete(predicate) ---------------------------------------------

        [Test]
        public void DeletePredicate_RemovesAllMatchingRows()
        {
            using (var seed = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(seed, "del");
                TestDataHelper.SeedMetric(seed, "del");
                TestDataHelper.SeedMetric(seed, "keep");
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);
            var unitOfWork = new UnitOfWork(factory);

            repo.Delete(m => m.Type == "del");
            unitOfWork.Commit();
            factory.Dispose();

            Assert.AreEqual(0, MetricCount("del"));
            Assert.AreEqual(1, MetricCount("keep"));
        }

        [Test]
        public void DeletePredicate_NoMatch_IsNoOp()
        {
            using (var seed = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(seed, "survivor");
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);
            var unitOfWork = new UnitOfWork(factory);

            repo.Delete(m => m.Type == "does-not-exist");
            unitOfWork.Commit();
            factory.Dispose();

            Assert.AreEqual(1, MetricCount("survivor"));
        }

        // ---- GetAll / GetMany (materialized snapshots) ---------------------

        [Test]
        public void GetAll_ReturnsMaterializedList_UnaffectedByLaterInserts()
        {
            using (var seed = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(seed, "a");
                TestDataHelper.SeedMetric(seed, "b");
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            var all = repo.GetAll();
            Assert.IsInstanceOf<List<Metric>>(all);
            Assert.AreEqual(2, all.Count());

            // A row inserted through a separate context AFTER the call is not in
            // the already-materialized collection.
            using (var later = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(later, "c");
            }
            Assert.AreEqual(2, all.Count());
            factory.Dispose();
        }

        [Test]
        public void GetMany_ReturnsMaterializedList_UnaffectedByLaterInserts()
        {
            using (var seed = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(seed, "match");
                TestDataHelper.SeedMetric(seed, "match");
                TestDataHelper.SeedMetric(seed, "other");
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            var many = repo.GetMany(m => m.Type == "match");
            Assert.IsInstanceOf<List<Metric>>(many);
            Assert.AreEqual(2, many.Count());

            using (var later = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(later, "match");
            }
            Assert.AreEqual(2, many.Count());
            factory.Dispose();
        }

        // ---- Get(predicate) -------------------------------------------------

        [Test]
        public void Get_Predicate_ReturnsFirstMatch()
        {
            using (var seed = new SocialGoalEntities())
            {
                TestDataHelper.SeedMetric(seed, "findme");
            }

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            var found = repo.Get(m => m.Type == "findme");
            Assert.IsNotNull(found);
            Assert.AreEqual("findme", found.Type);
            factory.Dispose();
        }

        [Test]
        public void Get_Predicate_NoMatch_ReturnsNull()
        {
            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            var found = repo.Get(m => m.Type == "nope");
            Assert.IsNull(found);
            factory.Dispose();
        }

        // ---- GetPage --------------------------------------------------------

        [Test]
        public void GetPage_ReturnsStaticPagedList_WithPageSliceAndFullTotal()
        {
            var orderedIds = new List<int>();
            using (var seed = new SocialGoalEntities())
            {
                for (int i = 0; i < 25; i++)
                {
                    orderedIds.Add(TestDataHelper.SeedMetric(seed, "p" + i.ToString("D2")).MetricId);
                }
            }
            orderedIds.Sort();

            var factory = new DatabaseFactory();
            var repo = new MetricRepository(factory);

            // Page 2 of 10, ordered by MetricId ascending.
            var page = repo.GetPage(new Page(2, 10), m => m.MetricId > 0, m => m.MetricId);

            Assert.IsInstanceOf<StaticPagedList<Metric>>(page);
            Assert.AreEqual(2, page.PageNumber);
            Assert.AreEqual(10, page.PageSize);
            // TotalItemCount is ALL rows matching the where, not just the page.
            Assert.AreEqual(25, page.TotalItemCount);
            Assert.AreEqual(10, page.Count);

            var expected = orderedIds.Skip(10).Take(10).ToList();
            CollectionAssert.AreEqual(expected, page.Select(m => m.MetricId).ToList());
            factory.Dispose();
        }

        // ---- DatabaseFactory ------------------------------------------------

        [Test]
        public void DatabaseFactory_Get_ReturnsSameContextInstance()
        {
            var factory = new DatabaseFactory();
            var first = factory.Get();
            var second = factory.Get();

            Assert.AreSame(first, second);
            factory.Dispose();
        }

        [Test]
        public void DatabaseFactory_AfterDispose_ContextIsDisposed()
        {
            var factory = new DatabaseFactory();
            var context = factory.Get();
            factory.Dispose();

            // Using a disposed DbContext throws. Pin the actual exception type.
            Assert.Throws<System.InvalidOperationException>(() => context.Metrics.ToList());
        }
    }
}
