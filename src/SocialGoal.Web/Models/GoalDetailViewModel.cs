namespace SocialGoal.Web.Models;

/// <summary>
/// Read model for the goal detail page (Sprint 5 vertical slice), shaped by
/// EF Core projection. GoalName is required (always present in the query);
/// the remaining members are genuinely optional display fields or value
/// types, so they stay defaulted.
/// </summary>
public class GoalDetailViewModel
{
    public int GoalId { get; set; }

    public required string GoalName { get; set; }

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
