using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class SecurityTokenConfiguration : IEntityTypeConfiguration<SecurityToken>
{
    public void Configure(EntityTypeBuilder<SecurityToken> builder)
    {
        builder.ToTable("SecurityTokens");
        builder.HasKey(t => t.SecurityTokenId);
    }
}
