namespace SocialGoal.Web.Data;

/// <summary>Comments -- a comment on an individual goal's update.</summary>
public class Comment
{
    public int CommentId { get; set; }

    public string? CommentText { get; set; }

    public int UpdateId { get; set; }

    public DateTime CommentDate { get; set; }

    public Update? Update { get; set; }
}
