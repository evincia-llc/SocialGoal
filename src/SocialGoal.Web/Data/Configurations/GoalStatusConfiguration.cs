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
    }
}
