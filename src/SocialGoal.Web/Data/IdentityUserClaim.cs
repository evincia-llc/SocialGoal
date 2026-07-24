namespace SocialGoal.Web.Data;

/// <summary>
/// AspNetUserClaims. Identity 1.0's IdentityUserClaim exposed only a
/// <c>User</c> navigation (no id property), so EF6 emitted the FK as the
/// shadow-shaped column <c>User_Id</c>. The CLR property is named
/// <see cref="UserId"/> -- the modern spelling -- and mapped explicitly to
/// that column; the column name is baseline, the property name is not.
/// </summary>
public class IdentityUserClaim
{
    public int Id { get; set; }

    public string? ClaimType { get; set; }

    public string? ClaimValue { get; set; }

    public required string UserId { get; set; }

    public ApplicationUser? User { get; set; }
}
