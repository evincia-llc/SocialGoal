using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GoalStatusConfiguration : IEntityTypeConfiguration<GoalStatus>
{
    public void Configure(EntityTypeBuilder<GoalStatus> builder)
    {
        // EF6 pluralization quirk pinned by the baseline: the table name stays
        // singular.
        builder.ToTable("GoalStatus");
        builder.HasKey(s => s.GoalStatusId);
        builder.Property(s => s.GoalStatusType).HasMaxLength(50);

        // Reference data with explicit ids (see MetricConfiguration for the
        // idempotency mechanism). GoalStatusId 1 MUST be "In Progress": the
        // legacy Goal and GroupGoal constructors hardcode GoalStatusId = 1,
        // and Sprint 7 reproduces that default in the services.
        builder.HasData(
            new GoalStatus { GoalStatusId = 1, GoalStatusType = "In Progress" },
            new GoalStatus { GoalStatusId = 2, GoalStatusType = "On Hold" },
            new GoalStatus { GoalStatusId = 3, GoalStatusType = "Completed" });
    }
}
