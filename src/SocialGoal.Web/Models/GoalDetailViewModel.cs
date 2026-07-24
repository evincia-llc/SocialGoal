namespace SocialGoal.Web.Models;

/// <summary>
/// Read model for the goal detail page (Sprint 5 vertical slice). Shaped by
/// EF Core projection -- properties are defaulted rather than `required`
/// because required members cannot be set inside a query expression tree.
/// </summary>
public class GoalDetailViewModel
{
    public int GoalId { get; set; }

    public string GoalName { get; set; } = string.Empty;

    public string? Desc { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double? Target { get; set; }

    public bool GoalType { get; set; }

    public string? MetricType { get; set; }

    public string? GoalStatusType { get; set; }

    public string? OwnerDisplayName { get; set; }

    public DateTime CreatedDate { get; set; }
}
