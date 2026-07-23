using System;
using System.Collections.Generic;
using System.Data.Entity;
using SocialGoal.Data.Models;
using SocialGoal.Model.Models;

namespace SocialGoal.Data
{
    /// <summary>
    /// Seeds lookup data (metrics, goal statuses) when the database is first
    /// created. Never drops or modifies an existing database.
    /// </summary>
    public class SampleDataCreateDatabaseIfNotExists : CreateDatabaseIfNotExists<SocialGoalEntities>
    {
        protected override void Seed(SocialGoalEntities context)
        {
            new List<Metric>
            {
                new Metric { Type ="%"},
                new Metric { Type ="$"},
                new Metric { Type ="$ M"},
                new Metric { Type ="Rs"},
                new Metric { Type ="Hours"},
                new Metric { Type ="Km"},
                new Metric { Type ="Kg"},
                new Metric { Type ="Years"}

            }.ForEach(m => context.Metrics.Add(m));

            new List<GoalStatus>
            {
                new GoalStatus{GoalStatusType="In Progress"},
                new GoalStatus{GoalStatusType="On Hold"},
                new GoalStatus{GoalStatusType="Completed"}
            }.ForEach(m => context.GoalStatus.Add(m));

            context.Commit();

        }

    }

    /// <summary>
    /// Maps the "DatabaseInitializer" appSetting to an EF initializer.
    /// "CreateIfNotExists" is an explicit local-dev opt-in; any other value,
    /// including absent config, means no initializer at all. Destructive
    /// initializers (drop/recreate) must never be added here.
    /// </summary>
    public static class DatabaseInitializerFactory
    {
        public static IDatabaseInitializer<SocialGoalEntities> FromSetting(string setting)
        {
            if (string.Equals(setting, "CreateIfNotExists", StringComparison.OrdinalIgnoreCase))
                return new SampleDataCreateDatabaseIfNotExists();
            return null;
        }
    }
}
