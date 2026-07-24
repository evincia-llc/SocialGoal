namespace SocialGoal.Web.Data;

/// <summary>
/// GroupUpdateUsers. <see cref="UserId"/> is a bare column with no FK
/// constraint in the baseline.
/// </summary>
public class GroupUpdateUser
{
    public int GroupUpdateUserId { get; set; }

    public int GroupUpdateId { get; set; }

    public required string UserId { get; set; }
}
