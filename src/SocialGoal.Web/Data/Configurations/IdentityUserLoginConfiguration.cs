using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin> builder)
    {
        builder.ToTable("AspNetUserLogins");
        builder.HasKey(l => new { l.UserId, l.LoginProvider, l.ProviderKey });
        builder.Property(l => l.UserId).HasMaxLength(128);
        builder.Property(l => l.LoginProvider).HasMaxLength(128);
        builder.Property(l => l.ProviderKey).HasMaxLength(128);

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("IdentityUserLogin_User");
    }
}
