using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class UpdateSupportConfiguration : IEntityTypeConfiguration<UpdateSupport>
{
    public void Configure(EntityTypeBuilder<UpdateSupport> builder)
    {
        builder.ToTable("UpdateSupports");
        builder.HasKey(s => s.UpdateSupportId);
        builder.Property(s => s.UpdateSupportedDate).HasColumnType("datetime");

        builder.HasOne(s => s.Update)
            .WithMany(u => u.UpdateSupports)
            .HasForeignKey(s => s.UpdateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("UpdateSupport_Update");
    }
}
