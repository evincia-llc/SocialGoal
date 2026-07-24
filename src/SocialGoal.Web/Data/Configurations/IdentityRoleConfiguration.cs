using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.ToTable("AspNetRoles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasMaxLength(128);
        builder.Property(r => r.Name).IsRequired();
    }
}
