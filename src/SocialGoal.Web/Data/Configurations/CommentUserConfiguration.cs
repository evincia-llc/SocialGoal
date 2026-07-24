using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class CommentUserConfiguration : IEntityTypeConfiguration<CommentUser>
{
    public void Configure(EntityTypeBuilder<CommentUser> builder)
    {
        builder.ToTable("CommentUsers");
        builder.HasKey(c => c.CommentUserId);
        // No relationship: the baseline leaves CommentId and UserId unconstrained.
        builder.Property(c => c.UserId).IsRequired();
    }
}
