using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupUserConfiguration : IEntityTypeConfiguration<GroupUser>
{
    public void Configure(EntityTypeBuilder<GroupUser> builder)
    {
        builder.ToTable("GroupUsers");
        builder.HasKey(u => u.GroupUserId);
        builder.Property(u => u.UserId).IsRequired();
        builder.Property(u => u.AddedDate).HasColumnType("datetime");
        // No FK on GroupId or UserId -- see GroupUser.
    }
}
