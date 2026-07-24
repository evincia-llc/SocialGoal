namespace SocialGoal.Web.Data;

/// <summary>Foci -- a group's focus area (EF6 pluralized Focus to Foci).</summary>
public class Focus
{
    public int FocusId { get; set; }

    public string? FocusName { get; set; }

    public string? Description { get; set; }

    public int GroupId { get; set; }

    public DateTime CreatedDate { get; set; }

    public Group? Group { get; set; }

    public ICollection<GroupGoal> GroupGoals { get; } = new List<GroupGoal>();
}
