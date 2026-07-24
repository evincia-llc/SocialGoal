using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class FollowRequestConfiguration : IEntityTypeConfiguration<FollowRequest>
{
    public void Configure(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.HasKey(f => f.FollowRequestId);
        builder.Property(f => f.FromUserId).IsRequired();
        builder.Property(f => f.ToUserId).IsRequired();
    }
}
