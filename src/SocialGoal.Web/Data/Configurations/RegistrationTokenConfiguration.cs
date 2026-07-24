using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class RegistrationTokenConfiguration : IEntityTypeConfiguration<RegistrationToken>
{
    public void Configure(EntityTypeBuilder<RegistrationToken> builder)
    {
        builder.ToTable("RegistrationTokens");
        builder.HasKey(t => t.RegistrationTokenId);
        builder.Property(t => t.Role).HasMaxLength(50);
    }
}
