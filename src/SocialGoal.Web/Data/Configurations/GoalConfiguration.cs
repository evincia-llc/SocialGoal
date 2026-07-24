using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("Goals");
        builder.HasKey(g => g.GoalId);
        builder.Property(g => g.GoalName).HasMaxLength(55).IsRequired();
        builder.Property(g => g.Desc).HasMaxLength(100);
        builder.Property(g => g.StartDate).HasColumnType("datetime");
        builder.Property(g => g.EndDate).HasColumnType("datetime");
        builder.Property(g => g.CreatedDate).HasColumnType("datetime");
        builder.Property(g => g.UserId).HasMaxLength(128);

        builder.HasOne(g => g.GoalStatus)
            .WithMany(s => s.Goals)
            .HasForeignKey(g => g.GoalStatusId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GoalStatus_Goals");
        builder.HasOne(g => g.Metric)
            .WithMany(m => m.Goals)
            .HasForeignKey(g => g.MetricId)
            .HasConstraintName("Metric_Goals");
        builder.HasOne(g => g.User)
            .WithMany(u => u.Goals)
            .HasForeignKey(g => g.UserId)
            .HasConstraintName("ApplicationUser_Goals");
    }
}
