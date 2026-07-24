using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupRequestConfiguration : IEntityTypeConfiguration<GroupRequest>
{
    public void Configure(EntityTypeBuilder<GroupRequest> builder)
    {
        builder.ToTable("GroupRequests");
        builder.HasKey(r => r.GroupRequestId);
        builder.Property(r => r.UserId).HasMaxLength(128);

        builder.HasOne(r => r.Group)
            .WithMany(g => g.GroupRequests)
            .HasForeignKey(r => r.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupRequest_Group");
        builder.HasOne(r => r.User)
            .WithMany(u => u.GroupRequests)
            .HasForeignKey(r => r.UserId)
            .HasConstraintName("ApplicationUser_GroupRequests");
    }
}
