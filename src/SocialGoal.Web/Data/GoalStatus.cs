namespace SocialGoal.Web.Data;

/// <summary>GoalStatus (singular table name -- an EF6 pluralizer quirk).</summary>
public class GoalStatus
{
    public int GoalStatusId { get; set; }

    public string? GoalStatusType { get; set; }

    public ICollection<Goal> Goals { get; } = new List<Goal>();

    public ICollection<GroupGoal> GroupGoals { get; } = new List<GroupGoal>();
}
