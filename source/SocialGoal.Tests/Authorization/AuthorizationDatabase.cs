using System.Data.Entity;
using NUnit.Framework;
using SocialGoal.Data.Models;
using SocialGoal.Mappings;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Namespace-level fixture for the behavioral authorization matrix. Mirrors
    /// <c>SocialGoal.Tests.Data.CharacterizationDatabase</c>: the framework
    /// initializer is disabled outright (repo hard rule forbids
    /// DropCreateDatabaseIfModelChanges and every IDatabaseInitializer), and the
    /// LocalDB schema is created and dropped by hand.
    ///
    /// It runs around the Authorization namespace ONLY; the Data fixture runs
    /// around the Data namespace ONLY. Both target the same dedicated LocalDB
    /// catalog (connection name "SocialGoalEntities" -> catalog
    /// SocialGoal_CharacterizationTests) but are disjoint in time -- NUnit walks
    /// each namespace subtree under its own SetUpFixture, so they never interleave.
    ///
    /// AutoMapper is initialized here once: controllers call the static
    /// <c>Mapper.Map</c> and the full profiles must be present before any action runs.
    /// </summary>
    [SetUpFixture]
    public class AuthorizationDatabase
    {
        [OneTimeSetUp]
        public void CreateSchema()
        {
            Database.SetInitializer<SocialGoalEntities>(null);

            using (var context = new SocialGoalEntities())
            {
                if (context.Database.Exists())
                {
                    context.Database.Delete();
                }
                context.Database.Create();
            }

            // Full domain <-> view-model profiles, exactly as the web host wires them.
            AutoMapperConfiguration.Configure();
        }

        [OneTimeTearDown]
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
