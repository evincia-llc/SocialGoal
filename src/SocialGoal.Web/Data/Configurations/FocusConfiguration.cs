using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class FocusConfiguration : IEntityTypeConfiguration<Focus>
{
    public void Configure(EntityTypeBuilder<Focus> builder)
    {
        // EF6 pluralized Focus to Foci; EF Core has no pluralizer, so the
        // baseline name is stated outright.
        builder.ToTable("Foci");
        builder.HasKey(f => f.FocusId);
        builder.Property(f => f.FocusName).HasMaxLength(50);
        builder.Property(f => f.Description).HasMaxLength(100);
        builder.Property(f => f.CreatedDate).HasColumnType("datetime");

        builder.HasOne(f => f.Group)
            .WithMany(g => g.Foci)
            .HasForeignKey(f => f.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("Group_Foci");
    }
}
