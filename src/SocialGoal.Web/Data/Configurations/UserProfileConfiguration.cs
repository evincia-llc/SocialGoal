using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");
        builder.HasKey(p => p.UserProfileId);
        builder.Property(p => p.DateEdited).HasColumnType("datetime");
        builder.Property(p => p.DateOfBirth).HasColumnType("datetime");
        builder.Property(p => p.FirstName).HasMaxLength(100);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.State).HasMaxLength(50);
        builder.Property(p => p.Country).HasMaxLength(50);
        // 50, not 128: the legacy config under-sized the column that stores an
        // Identity id. Reproduced, not corrected.
        builder.Property(p => p.UserId).HasMaxLength(50);
    }
}
