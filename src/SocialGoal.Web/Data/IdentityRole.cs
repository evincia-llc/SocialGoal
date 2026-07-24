namespace SocialGoal.Web.Data;

/// <summary>
/// AspNetRoles. A plain POCO standing in for Identity 1.0's IdentityRole so
/// the schema round-trips; Sprint 8 replaces it with ASP.NET Core Identity's
/// own role type.
/// </summary>
public class IdentityRole
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public ICollection<IdentityUserRole> Users { get; } = new List<IdentityUserRole>();
}
