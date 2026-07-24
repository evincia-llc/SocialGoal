using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SocialGoal.Data.Infrastructure;
using SocialGoal.Data.Models;
using SocialGoal.Data.Repository;
using SocialGoal.Model.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Characterization tests for GoalRepository.GetGoalsByPage
    /// (SocialGoal.Data/Repository/GoalRepository.cs). Pins the filter, sort and
    /// (zero-based) paging semantics against a real LocalDB. Behavioral pin only.
    /// </summary>
    [TestFixture]
    public class GoalRepositoryCharacterizationTests
    {
        private int _metricIdValue;   // shared FK prerequisites
        private int _statusId;
        private readonly List<DatabaseFactory> _factories = new List<DatabaseFactory>();

        [TearDown]
        public void DisposeFactories()
        {
            foreach (var factory in _factories)
                factory.Dispose();
            _factories.Clear();
        }

        [SetUp]
        public void CleanSlateAndSeedPrerequisites()
        {
            using (var context = new SocialGoalEntities())
            {
                TestDataHelper.CleanCoreTables(context);
                _metricIdValue = TestDataHelper.SeedMetric(context, "count").MetricId;
                _statusId = TestDataHelper.SeedGoalStatus(context, "Active").GoalStatusId;
            }
        }

        // ---- seeding helpers ------------------------------------------------

        private Goal SeedGoal(SocialGoalEntities context, string userId, bool groupGoal,
            string name, DateTime createdDate)
        {
            var goal = new Goal
            {
                GoalName = name,
                Desc = "desc",
                GoalType = groupGoal,
                GoalStatusId = _statusId,
                MetricId = _metricIdValue,
                UserId = userId,
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                CreatedDate = createdDate
            };
            context.Goals.Add(goal);
            context.SaveChanges();
            return goal;
        }

        private void SeedSupport(SocialGoalEntities context, int goalId, string userId)
        {
            context.Support.Add(new Support
            {
                GoalId = goalId,
                UserId = userId,
                SupportedDate = DateTime.Now
            });
            context.SaveChanges();
        }

        private void SeedFollow(SocialGoalEntities context, string fromUserId, string toUserId)
        {
            context.FollowUser.Add(new FollowUser
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Accepted = true,
                AddedDate = DateTime.Now
            });
            context.SaveChanges();
        }

        private GoalRepository NewRepo()
        {
            var factory = new DatabaseFactory();
            _factories.Add(factory);
            return new GoalRepository(factory);
        }

        // ---- tests ----------------------------------------------------------

        [Test]
        public void GetGoalsByPage_AlwaysExcludesGroupGoals()
        {
            int personalId, groupId;
            using (var context = new SocialGoalEntities())
            {
                var user = TestDataHelper.SeedUser(context, "owner1");
                personalId = SeedGoal(context, user.Id, false, "personal", DateTime.Now).GoalId;
                groupId = SeedGoal(context, user.Id, true, "group", DateTime.Now).GoalId;

                var result = NewRepo().GetGoalsByPage(user.Id, 0, 10, null, null).ToList();

                var ids = result.Select(g => g.GoalId).ToList();
                CollectionAssert.Contains(ids, personalId);
                CollectionAssert.DoesNotContain(ids, groupId);
            }
        }

        [Test]
        public void GetGoalsByPage_MyGoals_ReturnsOnlyUsersGoals()
        {
            using (var context = new SocialGoalEntities())
            {
                var u1 = TestDataHelper.SeedUser(context, "u1");
                var u2 = TestDataHelper.SeedUser(context, "u2");
                int mine = SeedGoal(context, u1.Id, false, "mine", DateTime.Now).GoalId;
                SeedGoal(context, u2.Id, false, "theirs", DateTime.Now);

                var result = NewRepo().GetGoalsByPage(u1.Id, 0, 10, null, "My Goals").ToList();

                CollectionAssert.AreEquivalent(new[] { mine }, result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_MyFollowingsGoals_ReturnsGoalsOfFollowedUsers()
        {
            using (var context = new SocialGoalEntities())
            {
                var me = TestDataHelper.SeedUser(context, "me");
                var followed = TestDataHelper.SeedUser(context, "followed");
                var stranger = TestDataHelper.SeedUser(context, "stranger");

                SeedFollow(context, me.Id, followed.Id);

                int followedGoal = SeedGoal(context, followed.Id, false, "followed-goal", DateTime.Now).GoalId;
                SeedGoal(context, me.Id, false, "my-own-goal", DateTime.Now);
                SeedGoal(context, stranger.Id, false, "stranger-goal", DateTime.Now);

                var result = NewRepo().GetGoalsByPage(me.Id, 0, 10, null, "My Followings Goals").ToList();

                CollectionAssert.AreEquivalent(new[] { followedGoal }, result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_MyFollowedGoals_ReturnsGoalsTheUserSupports()
        {
            using (var context = new SocialGoalEntities())
            {
                var me = TestDataHelper.SeedUser(context, "supporter");
                var author = TestDataHelper.SeedUser(context, "author");

                int supported = SeedGoal(context, author.Id, false, "supported", DateTime.Now).GoalId;
                int notSupported = SeedGoal(context, author.Id, false, "not-supported", DateTime.Now).GoalId;
                SeedSupport(context, supported, me.Id);

                var result = NewRepo().GetGoalsByPage(me.Id, 0, 10, null, "My Followed Goals").ToList();

                var ids = result.Select(g => g.GoalId).ToList();
                CollectionAssert.AreEquivalent(new[] { supported }, ids);
                CollectionAssert.DoesNotContain(ids, notSupported);
            }
        }

        [Test]
        public void GetGoalsByPage_UnknownFilter_ReturnsAllNonGroupGoals()
        {
            using (var context = new SocialGoalEntities())
            {
                var u = TestDataHelper.SeedUser(context, "anyuser");
                int g1 = SeedGoal(context, u.Id, false, "g1", DateTime.Now).GoalId;
                int g2 = SeedGoal(context, u.Id, false, "g2", DateTime.Now).GoalId;
                SeedGoal(context, u.Id, true, "group", DateTime.Now);

                var result = NewRepo().GetGoalsByPage(u.Id, 0, 10, null, "Some Unrecognized Filter").ToList();

                CollectionAssert.AreEquivalent(new[] { g1, g2 }, result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_SortByDate_OrdersByCreatedDateDescending()
        {
            using (var context = new SocialGoalEntities())
            {
                var u = TestDataHelper.SeedUser(context, "dateuser");
                int oldest = SeedGoal(context, u.Id, false, "oldest", new DateTime(2020, 1, 1)).GoalId;
                int middle = SeedGoal(context, u.Id, false, "middle", new DateTime(2021, 1, 1)).GoalId;
                int newest = SeedGoal(context, u.Id, false, "newest", new DateTime(2022, 1, 1)).GoalId;

                var result = NewRepo().GetGoalsByPage(u.Id, 0, 10, "Date", null).ToList();

                CollectionAssert.AreEqual(new[] { newest, middle, oldest },
                    result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_SortByPopularity_OrdersBySupportsCountDescending()
        {
            using (var context = new SocialGoalEntities())
            {
                var u = TestDataHelper.SeedUser(context, "popuser");
                int none = SeedGoal(context, u.Id, false, "none", DateTime.Now).GoalId;
                int two = SeedGoal(context, u.Id, false, "two", DateTime.Now).GoalId;
                int one = SeedGoal(context, u.Id, false, "one", DateTime.Now).GoalId;

                SeedSupport(context, two, "s1");
                SeedSupport(context, two, "s2");
                SeedSupport(context, one, "s3");

                var result = NewRepo().GetGoalsByPage(u.Id, 0, 10, "Popularity", null).ToList();

                CollectionAssert.AreEqual(new[] { two, one, none },
                    result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_SortByUnknown_AppliesNoExplicitOrdering_SetEqualityOnly()
        {
            using (var context = new SocialGoalEntities())
            {
                var u = TestDataHelper.SeedUser(context, "sortuser");
                int g1 = SeedGoal(context, u.Id, false, "g1", DateTime.Now).GoalId;
                int g2 = SeedGoal(context, u.Id, false, "g2", DateTime.Now).GoalId;
                int g3 = SeedGoal(context, u.Id, false, "g3", DateTime.Now).GoalId;

                var result = NewRepo().GetGoalsByPage(u.Id, 0, 10, "Alphabetical", null).ToList();

                CollectionAssert.AreEquivalent(new[] { g1, g2, g3 },
                    result.Select(g => g.GoalId).ToList());
            }
        }

        [Test]
        public void GetGoalsByPage_PagingIsZeroBased_SkipIsRecordsTimesCurrentPage()
        {
            using (var context = new SocialGoalEntities())
            {
                var u = TestDataHelper.SeedUser(context, "paguser");
                // Distinct CreatedDate so "Date" sort is deterministic (newest first).
                int g1 = SeedGoal(context, u.Id, false, "g1", new DateTime(2020, 1, 1)).GoalId;
                int g2 = SeedGoal(context, u.Id, false, "g2", new DateTime(2020, 2, 1)).GoalId;
                int g3 = SeedGoal(context, u.Id, false, "g3", new DateTime(2020, 3, 1)).GoalId;
                int g4 = SeedGoal(context, u.Id, false, "g4", new DateTime(2020, 4, 1)).GoalId;
                int g5 = SeedGoal(context, u.Id, false, "g5", new DateTime(2020, 5, 1)).GoalId;

                // currentPage = 0 => skip 0: first noOfRecords (2 newest).
                var page0 = NewRepo().GetGoalsByPage(u.Id, 0, 2, "Date", null)
                    .Select(g => g.GoalId).ToList();
                CollectionAssert.AreEqual(new[] { g5, g4 }, page0);

                // currentPage = 1 => skip noOfRecords (2): the next 2.
                var page1 = NewRepo().GetGoalsByPage(u.Id, 1, 2, "Date", null)
                    .Select(g => g.GoalId).ToList();
                CollectionAssert.AreEqual(new[] { g3, g2 }, page1);

                // currentPage = 2 => skip 4: the last 1.
                var page2 = NewRepo().GetGoalsByPage(u.Id, 2, 2, "Date", null)
                    .Select(g => g.GoalId).ToList();
                CollectionAssert.AreEqual(new[] { g1 }, page2);
            }
        }
    }
}
