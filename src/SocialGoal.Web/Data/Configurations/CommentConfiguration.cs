using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.CommentId);
        builder.Property(c => c.CommentText).HasMaxLength(250);
        builder.Property(c => c.CommentDate).HasColumnType("datetime");

        builder.HasOne(c => c.Update)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UpdateId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("Update_Comments");
    }
}
