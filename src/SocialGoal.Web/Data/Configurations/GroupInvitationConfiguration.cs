using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupInvitationConfiguration : IEntityTypeConfiguration<GroupInvitation>
{
    public void Configure(EntityTypeBuilder<GroupInvitation> builder)
    {
        builder.ToTable("GroupInvitations");
        builder.HasKey(i => i.GroupInvitationId);
        builder.Property(i => i.SentDate).HasColumnType("datetime");

        builder.HasOne(i => i.Group)
            .WithMany(g => g.GroupInvitations)
            .HasForeignKey(i => i.GroupId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("GroupInvitation_Group");
    }
}
