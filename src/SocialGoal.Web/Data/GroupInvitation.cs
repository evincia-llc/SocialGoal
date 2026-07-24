namespace SocialGoal.Web.Data;

/// <summary>
/// GroupInvitations. The user ids are bare nvarchar(max) columns; only the
/// group is constrained.
/// </summary>
public class GroupInvitation
{
    public int GroupInvitationId { get; set; }

    public string? FromUserId { get; set; }

    public int GroupId { get; set; }

    public string? ToUserId { get; set; }

    public DateTime SentDate { get; set; }

    public bool Accepted { get; set; }

    public Group? Group { get; set; }
}
