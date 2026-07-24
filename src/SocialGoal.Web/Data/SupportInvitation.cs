namespace SocialGoal.Web.Data;

/// <summary>SupportInvitations.</summary>
public class SupportInvitation
{
    public int SupportInvitationId { get; set; }

    public string? FromUserId { get; set; }

    public int GoalId { get; set; }

    public string? ToUserId { get; set; }

    public DateTime SentDate { get; set; }

    public bool Accepted { get; set; }

    public Goal? Goal { get; set; }
}
