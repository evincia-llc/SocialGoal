namespace SocialGoal.Web.Data;

/// <summary>
/// AspNetUserLogins. Composite key in baseline order:
/// (UserId, LoginProvider, ProviderKey).
/// </summary>
public class IdentityUserLogin
{
    public required string UserId { get; set; }

    public required string LoginProvider { get; set; }

    public required string ProviderKey { get; set; }

    public ApplicationUser? User { get; set; }
}
