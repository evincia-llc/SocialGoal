using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupUpdateSupportConfiguration : IEntityTypeConfiguration<GroupUpdateSupport>
{
    public void Configure(EntityTypeBuilder<GroupUpdateSupport> builder)
    {
        builder.ToTable("GroupUpdateSupports");
        builder.HasKey(s => s.GroupUpdateSupportId);
        builder.Property(s => s.UpdateSupportedDate).HasColumnType("datetime");

        // GroupUserId is deliberately unconstrained -- no navigation in the
        // legacy entity, no FK in the baseline.
        builder.HasOne(s => s.GroupUpdate)
            .WithMany()
            .HasForeignKey(s => s.GroupUpdateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupUpdateSupport_GroupUpdate");
    }
}
