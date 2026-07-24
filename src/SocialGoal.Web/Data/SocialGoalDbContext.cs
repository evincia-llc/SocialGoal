using Microsoft.EntityFrameworkCore;

namespace SocialGoal.Web.Data;

/// <summary>
/// EF Core context mapped to the schema baseline of record
/// (docs/schema/schema-baseline.sql, Sprint 2 / D14). The mapping target is
/// byte-for-byte column/constraint parity with the EF6-generated DDL for the
/// entities modeled here; SchemaParitySpikeTests proves it against a live
/// LocalDB catalog comparison. Grows into the full Sprint 6-7 context.
/// </summary>
public class SocialGoalDbContext(DbContextOptions<SocialGoalDbContext> options)
    : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<GoalStatus> GoalStatuses => Set<GoalStatus>();

    public DbSet<Metric> Metrics => Set<Metric>();

    public DbSet<FollowUser> FollowUsers => Set<FollowUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(user =>
        {
            user.ToTable("AspNetUsers");
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).HasMaxLength(128);
            user.Property(u => u.UserName).IsRequired();
            // TPH artifact of Identity 1.0 (see Entities.cs); reproduced so the
            // legacy app and the modern host can share the database until the
            // Sprint 8 Identity migration reshapes AspNetUsers deliberately.
            user.Property(u => u.Discriminator).HasMaxLength(128).IsRequired();
            user.Property(u => u.DateCreated).HasColumnType("datetime");
            user.Property(u => u.LastLoginTime).HasColumnType("datetime");

            // The two unpaired legacy collections: EF6 saw FollowFromUser /
            // FollowToUser as *new* relationships (no InverseProperty pairing
            // with FollowUser.FromUser/ToUser) and emitted shadow FK columns.
            user.HasMany(u => u.FollowFromUser)
                .WithOne()
                .HasForeignKey("ApplicationUser_Id")
                .HasConstraintName("ApplicationUser_FollowFromUser");
            user.HasMany(u => u.FollowToUser)
                .WithOne()
                .HasForeignKey("ApplicationUser_Id1")
                .HasConstraintName("ApplicationUser_FollowToUser");
        });

        modelBuilder.Entity<Goal>(goal =>
        {
            goal.ToTable("Goals");
            goal.HasKey(g => g.GoalId);
            goal.Property(g => g.GoalName).HasMaxLength(55).IsRequired();
            goal.Property(g => g.Desc).HasMaxLength(100);
            goal.Property(g => g.StartDate).HasColumnType("datetime");
            goal.Property(g => g.EndDate).HasColumnType("datetime");
            goal.Property(g => g.CreatedDate).HasColumnType("datetime");
            goal.Property(g => g.UserId).HasMaxLength(128);

            goal.HasOne(g => g.GoalStatus)
                .WithMany()
                .HasForeignKey(g => g.GoalStatusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("GoalStatus_Goals");
            goal.HasOne(g => g.Metric)
                .WithMany()
                .HasForeignKey(g => g.MetricId)
                .HasConstraintName("Metric_Goals");
            goal.HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId)
                .HasConstraintName("ApplicationUser_Goals");
        });

        modelBuilder.Entity<GoalStatus>(status =>
        {
            // EF6 pluralization quirk pinned by the baseline: table name stays
            // singular.
            status.ToTable("GoalStatus");
            status.HasKey(s => s.GoalStatusId);
            status.Property(s => s.GoalStatusType).HasMaxLength(50);
        });

        modelBuilder.Entity<Metric>(metric =>
        {
            metric.ToTable("Metrics");
            metric.HasKey(m => m.MetricId);
        });

        modelBuilder.Entity<FollowUser>(follow =>
        {
            follow.ToTable("FollowUsers");
            follow.HasKey(f => f.FollowUserId);
            follow.Property(f => f.ToUserId).HasMaxLength(128);
            follow.Property(f => f.FromUserId).HasMaxLength(128);
            follow.Property(f => f.AddedDate).HasColumnType("datetime");

            follow.HasOne(f => f.FromUser)
                .WithMany()
                .HasForeignKey(f => f.FromUserId)
                .HasConstraintName("FollowUser_FromUser");
            follow.HasOne(f => f.ToUser)
                .WithMany()
                .HasForeignKey(f => f.ToUserId)
                .HasConstraintName("FollowUser_ToUser");
        });
    }
}
