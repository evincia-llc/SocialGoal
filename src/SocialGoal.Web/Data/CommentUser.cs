namespace SocialGoal.Web.Data;

/// <summary>
/// CommentUsers. <see cref="UserId"/> holds an Identity id but carries no FK
/// constraint in the baseline -- the legacy entity declared no navigation, so
/// EF6 emitted a bare column. Reproduced as-is; tightening it is a schema
/// change and needs its own decision.
/// </summary>
public class CommentUser
{
    public int CommentUserId { get; set; }

    public int CommentId { get; set; }

    public required string UserId { get; set; }
}
