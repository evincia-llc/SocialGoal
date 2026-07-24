using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupCommentUserConfiguration : IEntityTypeConfiguration<GroupCommentUser>
{
    public void Configure(EntityTypeBuilder<GroupCommentUser> builder)
    {
        builder.ToTable("GroupCommentUsers");
        builder.HasKey(c => c.GroupCommentUserId);
        builder.Property(c => c.UserId).IsRequired();
    }
}
