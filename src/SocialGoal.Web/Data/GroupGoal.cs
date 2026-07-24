namespace SocialGoal.Web.Data;

/// <summary>
/// GroupGoals. Five FK constraints in the baseline (Focus, GoalStatus, Group,
/// GroupUser, Metric) -- but <see cref="AssignedGroupUserId"/> and
/// <see cref="AssignedTo"/> are bare columns: the legacy entity declared no
/// navigation for either, so EF6 constrained neither.
/// </summary>
public class GroupGoal
{
    public int GroupGoalId { get; set; }

    public string? GoalName { get; set; }

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double? Target { get; set; }

    public int? MetricId { get; set; }

    public int? FocusId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int GoalStatusId { get; set; }

    public int GroupUserId { get; set; }

    public int? AssignedGroupUserId { get; set; }

    public string? AssignedTo { get; set; }

    public int GroupId { get; set; }

    public GroupUser? GroupUser { get; set; }

    public Group? Group { get; set; }

    public Metric? Metric { get; set; }

    public Focus? Focus { get; set; }

    public GoalStatus? GoalStatus { get; set; }

    public ICollection<GroupUpdate> Updates { get; } = new List<GroupUpdate>();
}
