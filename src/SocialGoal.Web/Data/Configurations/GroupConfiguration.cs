using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.GroupId);
        builder.Property(g => g.GroupName).HasMaxLength(50);
        builder.Property(g => g.CreatedDate).HasColumnType("datetime");
    }
}
