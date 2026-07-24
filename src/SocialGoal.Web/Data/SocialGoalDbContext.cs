using Microsoft.EntityFrameworkCore;

namespace SocialGoal.Web.Data;

/// <summary>
/// EF Core context mapped to the schema baseline of record
/// (docs/schema/schema-baseline.sql, Sprint 2 / D14). The mapping target is the
/// EF6-*generated* model -- every column facet, constraint name and cascade
/// action in that DDL is a requirement, including the accidents (shadow FK
/// columns, unconstrained user-id columns, the Foci pluralization, the
/// under-sized UserProfiles.UserId). SchemaParityTests proves the whole
/// 30-table catalog matches.
///
/// Per-entity configuration lives in Data/Configurations (D17).
/// </summary>
public class SocialGoalDbContext(DbContextOptions<SocialGoalDbContext> options)
    : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<CommentUser> CommentUsers => Set<CommentUser>();

    public DbSet<Focus> Foci => Set<Focus>();

    public DbSet<FollowRequest> FollowRequests => Set<FollowRequest>();

    public DbSet<FollowUser> FollowUsers => Set<FollowUser>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<GoalStatus> GoalStatuses => Set<GoalStatus>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupComment> GroupComments => Set<GroupComment>();

    public DbSet<GroupCommentUser> GroupCommentUsers => Set<GroupCommentUser>();

    public DbSet<GroupGoal> GroupGoals => Set<GroupGoal>();

    public DbSet<GroupInvitation> GroupInvitations => Set<GroupInvitation>();

    public DbSet<GroupRequest> GroupRequests => Set<GroupRequest>();

    public DbSet<GroupUpdate> GroupUpdates => Set<GroupUpdate>();

    public DbSet<GroupUpdateSupport> GroupUpdateSupports => Set<GroupUpdateSupport>();

    public DbSet<GroupUpdateUser> GroupUpdateUsers => Set<GroupUpdateUser>();

    public DbSet<GroupUser> GroupUsers => Set<GroupUser>();

    public DbSet<IdentityRole> Roles => Set<IdentityRole>();

    public DbSet<IdentityUserClaim> UserClaims => Set<IdentityUserClaim>();

    public DbSet<IdentityUserLogin> UserLogins => Set<IdentityUserLogin>();

    public DbSet<IdentityUserRole> UserRoles => Set<IdentityUserRole>();

    public DbSet<Metric> Metrics => Set<Metric>();

    public DbSet<RegistrationToken> RegistrationTokens => Set<RegistrationToken>();

    public DbSet<SecurityToken> SecurityTokens => Set<SecurityToken>();

    public DbSet<Support> Supports => Set<Support>();

    public DbSet<SupportInvitation> SupportInvitations => Set<SupportInvitation>();

    public DbSet<Update> Updates => Set<Update>();

    public DbSet<UpdateSupport> UpdateSupports => Set<UpdateSupport>();

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SocialGoalDbContext).Assembly);
    }
}
