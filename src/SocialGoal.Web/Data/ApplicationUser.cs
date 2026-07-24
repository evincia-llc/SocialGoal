namespace SocialGoal.Web.Data;

/// <summary>
/// AspNetUsers. Identity 1.0's <c>IdentityUser</c> subclass ported as a plain
/// POCO; Sprint 8 replaces it with a real ASP.NET Core Identity user.
/// </summary>
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

    public ICollection<GroupRequest> GroupRequests { get; } = new List<GroupRequest>();
}
