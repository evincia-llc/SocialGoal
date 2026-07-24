using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class SupportConfiguration : IEntityTypeConfiguration<Support>
{
    public void Configure(EntityTypeBuilder<Support> builder)
    {
        builder.ToTable("Supports");
        builder.HasKey(s => s.SupportId);
        builder.Property(s => s.SupportedDate).HasColumnType("datetime");

        builder.HasOne(s => s.Goal)
            .WithMany(g => g.Supports)
            .HasForeignKey(s => s.GoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("Support_Goal");
    }
}
