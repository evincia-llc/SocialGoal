using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupUpdateUserConfiguration : IEntityTypeConfiguration<GroupUpdateUser>
{
    public void Configure(EntityTypeBuilder<GroupUpdateUser> builder)
    {
        builder.ToTable("GroupUpdateUsers");
        builder.HasKey(u => u.GroupUpdateUserId);
        builder.Property(u => u.UserId).IsRequired();
    }
}
