namespace SocialGoal.Web.Data;

/// <summary>Updates -- progress updates against an individual goal.</summary>
public class Update
{
    public int UpdateId { get; set; }

    public string? Updatemsg { get; set; }

    /// <summary>Mapped to the lower-case baseline column <c>status</c>.</summary>
    public double? Status { get; set; }

    public int GoalId { get; set; }

    public DateTime UpdateDate { get; set; }

    public Goal? Goal { get; set; }

    public ICollection<Comment> Comments { get; } = new List<Comment>();

    public ICollection<UpdateSupport> UpdateSupports { get; } = new List<UpdateSupport>();
}
