namespace SocialGoal.Web.Data;

/// <summary>Metrics.</summary>
public class Metric
{
    public int MetricId { get; set; }

    public string? Type { get; set; }

    public ICollection<Goal> Goals { get; } = new List<Goal>();

    public ICollection<GroupGoal> GroupGoals { get; } = new List<GroupGoal>();
}
