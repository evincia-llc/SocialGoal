namespace SocialGoal.Web.Data;

/// <summary>
/// GroupCommentUsers. <see cref="UserId"/> is a bare column with no FK
/// constraint in the baseline.
/// </summary>
public class GroupCommentUser
{
    public int GroupCommentUserId { get; set; }

    public int GroupCommentId { get; set; }

    public required string UserId { get; set; }
}
