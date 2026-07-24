using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class UpdateConfiguration : IEntityTypeConfiguration<Update>
{
    public void Configure(EntityTypeBuilder<Update> builder)
    {
        builder.ToTable("Updates");
        builder.HasKey(u => u.UpdateId);
        // The legacy property was lower-case `status`; the column keeps that
        // name, the CLR property does not.
        builder.Property(u => u.Status).HasColumnName("status");
        builder.Property(u => u.UpdateDate).HasColumnType("datetime");

        builder.HasOne(u => u.Goal)
            .WithMany(g => g.Updates)
            .HasForeignKey(u => u.GoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("Goal_Updates");
    }
}
