using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SocialGoal.Web.Data.Configurations;

internal sealed class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(m => m.MetricId);
    }
}
