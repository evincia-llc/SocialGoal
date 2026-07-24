using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SocialGoal.Data.Models;
using SocialGoal.Model.Models;

namespace SocialGoal.Tests.Data
{
    /// <summary>
    /// Helpers shared by the data-layer characterization fixtures: FK-safe row
    /// cleanup, seeding of common prerequisites (user / metric / goal status),
    /// and runtime resolution of the tables EF actually generated. Table names
    /// are read from the model metadata rather than hardcoded so cleanup stays
    /// correct regardless of EF's pluralization.
    /// </summary>
    internal static class TestDataHelper
    {
        // ---- Table-name resolution from EF metadata -------------------------

        private static readonly Dictionary<Type, string> TableNameCache = new Dictionary<Type, string>();

        /// <summary>
        /// The fully-qualified [schema].[table] EF mapped <typeparamref name="T"/> to.
        /// </summary>
        internal static string QualifiedTable<T>(DbContext context) where T : class
        {
            string cached;
            if (TableNameCache.TryGetValue(typeof(T), out cached))
            {
                return cached;
            }

            // Resolve the CLR type to its entity name(s). The store set that
            // holds the rows is named after the entity TYPE (e.g. "Goal"),
            // whereas the conceptual set is named after the DbSet property (e.g.
            // "Goals") -- so we match the store set by element type, not by the
            // conceptual set name. The set's element type may be a BASE of T
            // (ApplicationUser rows live in the "IdentityUser" store set), hence
            // the whole base-type chain is a candidate.
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var objectItemCollection = (ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace);
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace)
                .Single(e => objectItemCollection.GetClrType(e) == typeof(T));

            var candidateNames = new HashSet<string>();
            for (EntityType t = entityType; t != null; t = t.BaseType as EntityType)
            {
                candidateNames.Add(t.Name);
            }

            var storeSet = metadata.GetItems<EntityContainer>(DataSpace.SSpace)
                .Single().BaseEntitySets.OfType<EntitySet>()
                .First(s => candidateNames.Contains(s.ElementType.Name));

            string table = storeSet.MetadataProperties.Contains("Table")
                && storeSet.MetadataProperties["Table"].Value != null
                ? (string)storeSet.MetadataProperties["Table"].Value
                : storeSet.Name;
            string schema = storeSet.Schema ?? "dbo";

            var qualified = "[" + schema + "].[" + table + "]";
            TableNameCache[typeof(T)] = qualified;
            return qualified;
        }

        /// <summary>
        /// Store entity-set name -> (schema, table) for every table in the model,
        /// read from the store (SSpace) metadata. This is the raw material for
        /// the model snapshot test. Uses the store space directly rather than the
        /// C-S mapping API, which is only public from EF 6.1 (this solution pins
        /// EF 6.0.x).
        /// </summary>
        internal static SortedDictionary<string, Tuple<string, string>> GetEntitySetTableMap(DbContext context)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            var map = new SortedDictionary<string, Tuple<string, string>>(StringComparer.Ordinal);
            foreach (var storeSet in metadata.GetItems<EntityContainer>(DataSpace.SSpace).Single().BaseEntitySets.OfType<EntitySet>())
            {
                string table = storeSet.MetadataProperties.Contains("Table")
                    && storeSet.MetadataProperties["Table"].Value != null
                    ? (string)storeSet.MetadataProperties["Table"].Value
                    : storeSet.Name;
                string schema = storeSet.Schema ?? "dbo";
                map[storeSet.Name] = Tuple.Create(schema, table);
            }
            return map;
        }

        // ---- Cleanup --------------------------------------------------------

        /// <summary>
        /// Deletes every row from the tables the characterization fixtures touch,
        /// children before parents to respect FK constraints. Called from each
        /// fixture's [SetUp] so fixtures are order-independent.
        /// </summary>
        internal static void CleanCoreTables(SocialGoalEntities context)
        {
            // Child -> parent order.
            Delete<Support>(context);
            Delete<FollowUser>(context);
            Delete<Goal>(context);
            Delete<GoalStatus>(context);
            Delete<Metric>(context);
            Delete<ApplicationUser>(context);
        }

        private static void Delete<T>(SocialGoalEntities context) where T : class
        {
            context.Database.ExecuteSqlCommand("DELETE FROM " + QualifiedTable<T>(context));
        }

        // ---- Seeding --------------------------------------------------------

        /// <summary>
        /// Adds an ApplicationUser (unique UserName required by Identity) and
        /// commits it. Returns the persisted user's Id.
        /// </summary>
        internal static ApplicationUser SeedUser(SocialGoalEntities context, string userName)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = userName + "@example.test",
                FirstName = "F",
                LastName = "L"
            };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        /// <summary>Adds and commits a Metric, returning it (MetricId populated).</summary>
        internal static Metric SeedMetric(SocialGoalEntities context, string type)
        {
            var metric = new Metric { Type = type };
            context.Metrics.Add(metric);
            context.SaveChanges();
            return metric;
        }

        /// <summary>Adds and commits a GoalStatus, returning it (GoalStatusId populated).</summary>
        internal static GoalStatus SeedGoalStatus(SocialGoalEntities context, string statusType)
        {
            var status = new GoalStatus { GoalStatusType = statusType };
            context.GoalStatus.Add(status);
            context.SaveChanges();
            return status;
        }
    }
}
