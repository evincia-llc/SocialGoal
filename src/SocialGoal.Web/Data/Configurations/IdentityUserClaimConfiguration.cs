using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
    {
        builder.ToTable("AspNetUserClaims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserId)
            .HasColumnName("User_Id")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("IdentityUserClaim_User");
    }
}
