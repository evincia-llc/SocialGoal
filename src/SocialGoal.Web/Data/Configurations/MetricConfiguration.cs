using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(m => m.MetricId);

        // Reference data, seeded with explicit ids so it is idempotent by
        // construction: HasData on an identity column emits IDENTITY_INSERT-
        // wrapped inserts in the migration, and re-running Migrate() on an
        // already-migrated database is a no-op. Ids and values reproduce the
        // legacy seed (SocialGoal.Data/DatabaseInitializers.cs): same values in
        // the same order, but the ids the legacy seed left to identity
        // ordering are stated outright here -- existing goal rows reference
        // them by id, so they are data, not an implementation detail.
        builder.HasData(
            new Metric { MetricId = 1, Type = "%" },
            new Metric { MetricId = 2, Type = "$" },
            new Metric { MetricId = 3, Type = "$ M" },
            new Metric { MetricId = 4, Type = "Rs" },
            new Metric { MetricId = 5, Type = "Hours" },
            new Metric { MetricId = 6, Type = "Km" },
            new Metric { MetricId = 7, Type = "Kg" },
            new Metric { MetricId = 8, Type = "Years" });
    }
}
