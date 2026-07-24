namespace SocialGoal.Web.Data;

// Sprint 5 spike slice of the domain model: Goal + its lookups, ApplicationUser,
// and FollowUser (the gnarly relationship: four FKs to AspNetUsers, two of them
// EF6 shadow-column accidents). Property names mirror the legacy entities so the
// Sprint 6-7 port stays mechanical; column facets live in SocialGoalDbContext.

public class ApplicationUser
{
    public required string Id { get; set; }

    public required string UserName { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    // Everything below is nullable in the database even where the legacy CLR
    // type is not: Identity 1.0's IdentityDbContext modeled IdentityUser ->
    // ApplicationUser as TPH, so all ApplicationUser-declared columns were
    // emitted nullable alongside a Discriminator column. Legacy rows carry
    // Discriminator = "ApplicationUser" and EF6 will not materialize rows
    // without it.
    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? ProfilePicUrl { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public bool? Activated { get; set; }

    public int? RoleId { get; set; }

    public string Discriminator { get; set; } = "ApplicationUser";

    public ICollection<Goal> Goals { get; } = new List<Goal>();

    public ICollection<FollowUser> FollowFromUser { get; } = new List<FollowUser>();

    public ICollection<FollowUser> FollowToUser { get; } = new List<FollowUser>();
}

public class Goal
{
    public int GoalId { get; set; }

    public required string GoalName { get; set; }

    public string? Desc { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double? Target { get; set; }

    public bool GoalType { get; set; }

    public int? MetricId { get; set; }

    public int GoalStatusId { get; set; }

    public string? UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public ApplicationUser? User { get; set; }

    public Metric? Metric { get; set; }

    public GoalStatus? GoalStatus { get; set; }
}

public class GoalStatus
{
    public int GoalStatusId { get; set; }

    public string? GoalStatusType { get; set; }
}

public class Metric
{
    public int MetricId { get; set; }

    public string? Type { get; set; }
}

public class FollowUser
{
    public int FollowUserId { get; set; }

    public string? ToUserId { get; set; }

    public string? FromUserId { get; set; }

    public bool Accepted { get; set; }

    public DateTime AddedDate { get; set; }

    public ApplicationUser? ToUser { get; set; }

    public ApplicationUser? FromUser { get; set; }
}
