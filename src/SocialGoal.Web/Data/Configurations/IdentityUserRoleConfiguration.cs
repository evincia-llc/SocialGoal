using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole> builder)
    {
        builder.ToTable("AspNetUserRoles");
        builder.HasKey(r => new { r.UserId, r.RoleId });
        builder.Property(r => r.UserId).HasMaxLength(128);
        builder.Property(r => r.RoleId).HasMaxLength(128);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("IdentityUserRole_User");
        builder.HasOne(r => r.Role)
            .WithMany(role => role.Users)
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("IdentityUserRole_Role");
    }
}
