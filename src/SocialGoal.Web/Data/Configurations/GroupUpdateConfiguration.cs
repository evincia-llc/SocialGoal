using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupUpdateConfiguration : IEntityTypeConfiguration<GroupUpdate>
{
    public void Configure(EntityTypeBuilder<GroupUpdate> builder)
    {
        builder.ToTable("GroupUpdates");
        builder.HasKey(u => u.GroupUpdateId);
        // The legacy property was lower-case `status`; the column keeps that
        // name, the CLR property does not.
        builder.Property(u => u.Status).HasColumnName("status");
        builder.Property(u => u.UpdateDate).HasColumnType("datetime");

        builder.HasOne(u => u.GroupGoal)
            .WithMany(g => g.Updates)
            .HasForeignKey(u => u.GroupGoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupUpdate_GroupGoal");
    }
}
