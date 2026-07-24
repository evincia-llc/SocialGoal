using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupGoalConfiguration : IEntityTypeConfiguration<GroupGoal>
{
    public void Configure(EntityTypeBuilder<GroupGoal> builder)
    {
        builder.ToTable("GroupGoals");
        builder.HasKey(g => g.GroupGoalId);
        builder.Property(g => g.GoalName).HasMaxLength(50);
        builder.Property(g => g.Description).HasMaxLength(100);
        builder.Property(g => g.StartDate).HasColumnType("datetime");
        builder.Property(g => g.EndDate).HasColumnType("datetime");
        builder.Property(g => g.CreatedDate).HasColumnType("datetime");

        // AssignedGroupUserId and AssignedTo stay unconstrained: the legacy
        // entity gave neither a navigation, so the baseline has no FK for them.
        builder.HasOne(g => g.GoalStatus)
            .WithMany(s => s.GroupGoals)
            .HasForeignKey(g => g.GoalStatusId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupGoal_GoalStatus");
        builder.HasOne(g => g.Group)
            .WithMany()
            .HasForeignKey(g => g.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupGoal_Group");
        builder.HasOne(g => g.GroupUser)
            .WithMany(u => u.GroupGoals)
            .HasForeignKey(g => g.GroupUserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupUser_GroupGoals");
        builder.HasOne(g => g.Focus)
            .WithMany(f => f.GroupGoals)
            .HasForeignKey(g => g.FocusId)
            .HasConstraintName("Focus_GroupGoals");
        builder.HasOne(g => g.Metric)
            .WithMany(m => m.GroupGoals)
            .HasForeignKey(g => g.MetricId)
            .HasConstraintName("Metric_GroupGoals");
    }
}
