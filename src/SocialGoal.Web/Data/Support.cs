namespace SocialGoal.Web.Data;

/// <summary>
/// Supports -- a user backing someone else's goal. The goal is constrained;
/// <see cref="UserId"/> is a bare nvarchar(max) column.
/// </summary>
public class Support
{
    public int SupportId { get; set; }

    public int GoalId { get; set; }

    public string? UserId { get; set; }

    public DateTime SupportedDate { get; set; }

    public Goal? Goal { get; set; }
}
