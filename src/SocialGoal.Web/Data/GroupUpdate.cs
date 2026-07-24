namespace SocialGoal.Web.Data;

/// <summary>GroupUpdates -- progress updates against a group goal.</summary>
public class GroupUpdate
{
    public int GroupUpdateId { get; set; }

    public string? Updatemsg { get; set; }

    /// <summary>Mapped to the lower-case baseline column <c>status</c>.</summary>
    public double? Status { get; set; }

    public int GroupGoalId { get; set; }

    public DateTime UpdateDate { get; set; }

    public GroupGoal? GroupGoal { get; set; }

    public ICollection<GroupComment> GroupComments { get; } = new List<GroupComment>();
}
