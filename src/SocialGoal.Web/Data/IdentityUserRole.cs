namespace SocialGoal.Web.Data;

/// <summary>
/// AspNetUserRoles. Composite key in baseline order: (UserId, RoleId).
/// </summary>
public class IdentityUserRole
{
    public required string UserId { get; set; }

    public required string RoleId { get; set; }

    public ApplicationUser? User { get; set; }

    public IdentityRole? Role { get; set; }
}
