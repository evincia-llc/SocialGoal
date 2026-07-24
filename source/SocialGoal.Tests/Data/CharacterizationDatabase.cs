using System.Data.Entity;
using NUnit.Framework;
using SocialGoal.Data.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Namespace-level fixture for the data-layer characterization suite.
    /// Runs once for everything in SocialGoal.Tests.Data: it stands up a fresh
    /// LocalDB schema before any fixture runs and drops it afterwards.
    ///
    /// Lifecycle is EXPLICIT on purpose. The repo hard rule forbids
    /// DropCreateDatabaseIfModelChanges (and every IDatabaseInitializer): those
    /// mutate whatever database the connection string points at, silently, on a
    /// model hash mismatch. Here the initializer is disabled outright
    /// (SetInitializer null) and the schema is created and torn down by hand.
    /// The connection string (App.config, name "SocialGoalEntities") targets a
    /// dedicated LocalDB catalog, never real data.
    /// </summary>
    [SetUpFixture]
    public class CharacterizationDatabase
    {
        [SetUp]
        public void CreateSchema()
        {
            // Kill any framework initializer; we own the lifecycle.
            Database.SetInitializer<SocialGoalEntities>(null);

            using (var context = new SocialGoalEntities())
            {
                if (context.Database.Exists())
                {
                    context.Database.Delete();
                }
                context.Database.Create();
            }
        }

        [TearDown]
        public void DropSchema()
        {
            using (var context = new SocialGoalEntities())
            {
                if (context.Database.Exists())
                {
                    context.Database.Delete();
                }
            }
        }
    }
}
