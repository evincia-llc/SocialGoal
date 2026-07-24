using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class FollowUserConfiguration : IEntityTypeConfiguration<FollowUser>
{
    public void Configure(EntityTypeBuilder<FollowUser> builder)
    {
        builder.ToTable("FollowUsers");
        builder.HasKey(f => f.FollowUserId);
        builder.Property(f => f.ToUserId).HasMaxLength(128);
        builder.Property(f => f.FromUserId).HasMaxLength(128);
        builder.Property(f => f.AddedDate).HasColumnType("datetime");

        builder.HasOne(f => f.FromUser)
            .WithMany()
            .HasForeignKey(f => f.FromUserId)
            .HasConstraintName("FollowUser_FromUser");
        builder.HasOne(f => f.ToUser)
            .WithMany()
            .HasForeignKey(f => f.ToUserId)
            .HasConstraintName("FollowUser_ToUser");
    }
}
