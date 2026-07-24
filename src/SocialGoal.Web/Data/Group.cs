namespace SocialGoal.Web.Data;

/// <summary>Groups.</summary>
public class Group
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public ICollection<Focus> Foci { get; } = new List<Focus>();

    public ICollection<GroupInvitation> GroupInvitations { get; } = new List<GroupInvitation>();

    public ICollection<GroupRequest> GroupRequests { get; } = new List<GroupRequest>();
}
