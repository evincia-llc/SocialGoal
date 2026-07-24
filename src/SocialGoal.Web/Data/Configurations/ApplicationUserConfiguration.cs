using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasMaxLength(128);
        builder.Property(u => u.UserName).IsRequired();
        // TPH artifact of Identity 1.0 (see ApplicationUser); reproduced so the
        // legacy app and the modern host can share the database until the
        // Sprint 8 Identity migration reshapes AspNetUsers deliberately.
        builder.Property(u => u.Discriminator).HasMaxLength(128).IsRequired();
        builder.Property(u => u.DateCreated).HasColumnType("datetime");
        builder.Property(u => u.LastLoginTime).HasColumnType("datetime");

        // The two unpaired legacy collections: EF6 saw FollowFromUser /
        // FollowToUser as *new* relationships (no InverseProperty pairing with
        // FollowUser.FromUser/ToUser) and emitted shadow FK columns.
        builder.HasMany(u => u.FollowFromUser)
            .WithOne()
            .HasForeignKey("ApplicationUser_Id")
            .HasConstraintName("ApplicationUser_FollowFromUser");
        builder.HasMany(u => u.FollowToUser)
            .WithOne()
            .HasForeignKey("ApplicationUser_Id1")
            .HasConstraintName("ApplicationUser_FollowToUser");
    }
}
