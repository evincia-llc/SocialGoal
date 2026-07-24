using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using NUnit.Framework;
using SocialGoal.Data.Models;
using SocialGoal.Model.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Characterization of the EF model SocialGoalEntities.OnModelCreating
    /// actually produces: which entities land in the model and the tables they
    /// map to. Pins the real generated names (EF pluralization included), not
    /// what the configuration classes appear to intend. Behavioral pin only.
    /// </summary>
    [TestFixture]
    public class MappingSmokeTests
    {
        /// <summary>
        /// The full model snapshot: conceptual entity-set name -> schema.table,
        /// captured from the generated model. Every registered configuration,
        /// every DbSet, the Identity tables, and RegistrationToken (registered
        /// without a DbSet) are represented here.
        /// </summary>
        private static readonly SortedDictionary<string, string> ExpectedModel =
            new SortedDictionary<string, string>(System.StringComparer.Ordinal)
        {
            { "Comment", "dbo.Comments" },
            { "CommentUser", "dbo.CommentUsers" },
            { "Focus", "dbo.Foci" },                 // EF pluralizes Focus -> Foci
            { "FollowRequest", "dbo.FollowRequests" },
            { "FollowUser", "dbo.FollowUsers" },
            { "Goal", "dbo.Goals" },
            { "GoalStatus", "dbo.GoalStatus" },       // EF leaves GoalStatus unchanged
            { "Group", "dbo.Groups" },
            { "GroupComment", "dbo.GroupComments" },
            { "GroupCommentUser", "dbo.GroupCommentUsers" },
            { "GroupGoal", "dbo.GroupGoals" },
            { "GroupInvitation", "dbo.GroupInvitations" },
            { "GroupRequest", "dbo.GroupRequests" },
            { "GroupUpdate", "dbo.GroupUpdates" },
            { "GroupUpdateSupport", "dbo.GroupUpdateSupports" },
            { "GroupUpdateUser", "dbo.GroupUpdateUsers" },
            { "GroupUser", "dbo.GroupUsers" },
            { "IdentityRole", "dbo.AspNetRoles" },
            { "IdentityUser", "dbo.AspNetUsers" },
            { "IdentityUserClaim", "dbo.AspNetUserClaims" },
            { "IdentityUserLogin", "dbo.AspNetUserLogins" },
            { "IdentityUserRole", "dbo.AspNetUserRoles" },
            { "Metric", "dbo.Metrics" },
            { "RegistrationToken", "dbo.RegistrationTokens" },
            { "SecurityToken", "dbo.SecurityTokens" },
            { "Support", "dbo.Supports" },
            { "SupportInvitation", "dbo.SupportInvitations" },
            { "Update", "dbo.Updates" },
            { "UpdateSupport", "dbo.UpdateSupports" },
            { "UserProfile", "dbo.UserProfiles" },
        };

        /// <summary>
        /// Entities registered via modelBuilder.Configurations.Add in
        /// OnModelCreating (23 registrations), and the tables they map to.
        /// </summary>
        private static readonly SortedDictionary<string, string> ExpectedRegisteredConfigTables =
            new SortedDictionary<string, string>(System.StringComparer.Ordinal)
        {
            { "Comment", "dbo.Comments" },
            { "CommentUser", "dbo.CommentUsers" },
            { "Focus", "dbo.Foci" },
            { "FollowRequest", "dbo.FollowRequests" },
            { "FollowUser", "dbo.FollowUsers" },
            { "Goal", "dbo.Goals" },
            { "GoalStatus", "dbo.GoalStatus" },
            { "GroupComment", "dbo.GroupComments" },
            { "GroupCommentUser", "dbo.GroupCommentUsers" },
            { "Group", "dbo.Groups" },
            { "GroupGoal", "dbo.GroupGoals" },
            { "GroupInvitation", "dbo.GroupInvitations" },
            { "GroupRequest", "dbo.GroupRequests" },
            { "GroupUpdateSupport", "dbo.GroupUpdateSupports" },
            { "GroupUpdateUser", "dbo.GroupUpdateUsers" },
            { "GroupUser", "dbo.GroupUsers" },
            { "Metric", "dbo.Metrics" },
            { "RegistrationToken", "dbo.RegistrationTokens" },
            { "Support", "dbo.Supports" },
            { "SupportInvitation", "dbo.SupportInvitations" },
            { "Update", "dbo.Updates" },
            { "UpdateSupport", "dbo.UpdateSupports" },
            { "UserProfile", "dbo.UserProfiles" },
        };

        private static SortedDictionary<string, string> ActualModel(SocialGoalEntities context)
        {
            var map = TestDataHelper.GetEntitySetTableMap(context);
            var flat = new SortedDictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var kv in map)
            {
                flat[kv.Key] = kv.Value.Item1 + "." + kv.Value.Item2;
            }
            return flat;
        }

        [Test]
        public void EveryRegisteredConfiguration_IsInModel_AndMappedToItsActualTable()
        {
            using (var context = new SocialGoalEntities())
            {
                var actual = ActualModel(context);
                foreach (var expected in ExpectedRegisteredConfigTables)
                {
                    Assert.IsTrue(actual.ContainsKey(expected.Key),
                        "Entity set '" + expected.Key + "' from a registered configuration is missing from the model.");
                    Assert.AreEqual(expected.Value, actual[expected.Key],
                        "Table mapping for '" + expected.Key + "' differs from the pinned value.");
                }
            }
        }

        [Test]
        public void ApplicationUserConfiguration_IsNotApplied_ModelShowsDefaults()
        {
            // ApplicationUserConfiguration (unregistered) would make FirstName
            // required (non-nullable) with MaxLength 1. The generated model must
            // show the CONVENTION default instead: nullable, and not length 1.
            using (var context = new SocialGoalEntities())
            {
                var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
                // ApplicationUser and UserProfile both carry a FirstName; the
                // user entity is the one that also has the Identity-only
                // PasswordHash column.
                var userType = metadata.GetItems<EntityType>(DataSpace.CSpace)
                    .Single(et => et.Properties.Any(p => p.Name == "FirstName")
                               && et.Properties.Any(p => p.Name == "PasswordHash"));
                var firstName = userType.Properties.Single(p => p.Name == "FirstName");

                Assert.IsTrue(firstName.Nullable,
                    "FirstName is non-nullable, which means ApplicationUserConfiguration.IsRequired() WAS applied.");

                var maxLengthFacet = firstName.TypeUsage.Facets.FirstOrDefault(f => f.Name == "MaxLength");
                bool lengthIsOne = maxLengthFacet != null
                    && maxLengthFacet.Value != null
                    && maxLengthFacet.Value.Equals(1);
                Assert.IsFalse(lengthIsOne,
                    "FirstName MaxLength is 1, which means ApplicationUserConfiguration.HasMaxLength(1) WAS applied.");
            }
        }

        [Test]
        public void RegistrationToken_IsInModelViaConfiguration_ButHasNoDbSet()
        {
            using (var context = new SocialGoalEntities())
            {
                var actual = ActualModel(context);

                // Registered configuration alone puts the entity (and its table)
                // into the model, even without a DbSet property.
                Assert.IsTrue(actual.ContainsKey("RegistrationToken"),
                    "RegistrationToken should be in the model via its registered configuration.");
                Assert.AreEqual("dbo.RegistrationTokens", actual["RegistrationToken"]);
            }

            // The context exposes no DbSet<RegistrationToken> / IDbSet<RegistrationToken>.
            bool hasDbSet = typeof(SocialGoalEntities).GetProperties()
                .Any(p => p.PropertyType == typeof(DbSet<RegistrationToken>)
                       || p.PropertyType == typeof(IDbSet<RegistrationToken>));
            Assert.IsFalse(hasDbSet,
                "SocialGoalEntities unexpectedly exposes a DbSet for RegistrationToken.");
        }

        [Test]
        public void GeneratedModel_MatchesCommittedSnapshot()
        {
            using (var context = new SocialGoalEntities())
            {
                var actual = ActualModel(context);
                CollectionAssert.AreEqual(ExpectedModel, actual,
                    "The generated entity-set/table model drifted from the committed snapshot.");
            }
        }
    }
}
