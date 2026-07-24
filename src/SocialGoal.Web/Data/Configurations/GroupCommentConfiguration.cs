using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupCommentConfiguration : IEntityTypeConfiguration<GroupComment>
{
    public void Configure(EntityTypeBuilder<GroupComment> builder)
    {
        builder.ToTable("GroupComments");
        builder.HasKey(c => c.GroupCommentId);
        // No max length: unlike Comment.CommentText, the legacy config never
        // capped this one, so the baseline column is nvarchar(max).
        builder.Property(c => c.CommentDate).HasColumnType("datetime");

        builder.HasOne(c => c.GroupUpdate)
            .WithMany(u => u.GroupComments)
            .HasForeignKey(c => c.GroupUpdateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupComment_GroupUpdate");
    }
}
