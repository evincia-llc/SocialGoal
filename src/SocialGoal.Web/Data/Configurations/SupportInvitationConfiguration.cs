using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class SupportInvitationConfiguration : IEntityTypeConfiguration<SupportInvitation>
{
    public void Configure(EntityTypeBuilder<SupportInvitation> builder)
    {
        builder.ToTable("SupportInvitations");
        builder.HasKey(i => i.SupportInvitationId);
        builder.Property(i => i.SentDate).HasColumnType("datetime");

        builder.HasOne(i => i.Goal)
            .WithMany(g => g.SupportInvitations)
            .HasForeignKey(i => i.GoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("SupportInvitation_Goal");
    }
}
