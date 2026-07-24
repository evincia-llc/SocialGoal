namespace SocialGoal.Web.Data;

/// <summary>Goals.</summary>
public class Goal
{
    public int GoalId { get; set; }

    public required string GoalName { get; set; }

    public string? Desc { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double? Target { get; set; }

    public bool GoalType { get; set; }

    public int? MetricId { get; set; }

    public int GoalStatusId { get; set; }

    public string? UserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public ApplicationUser? User { get; set; }

    public Metric? Metric { get; set; }

    public GoalStatus? GoalStatus { get; set; }

    public ICollection<Update> Updates { get; } = new List<Update>();

    public ICollection<Support> Supports { get; } = new List<Support>();

    public ICollection<SupportInvitation> SupportInvitations { get; } = new List<SupportInvitation>();
}
