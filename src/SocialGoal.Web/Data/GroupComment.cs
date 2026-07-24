namespace SocialGoal.Web.Data;

/// <summary>
/// GroupComments. Note the asymmetry with <see cref="Comment"/>: the legacy
/// fluent config capped Comment.CommentText at 250 characters but never
/// registered one for this entity, so the baseline column is nvarchar(max).
/// </summary>
public class GroupComment
{
    public int GroupCommentId { get; set; }

    public string? CommentText { get; set; }

    public int GroupUpdateId { get; set; }

    public DateTime CommentDate { get; set; }

    public GroupUpdate? GroupUpdate { get; set; }
}
