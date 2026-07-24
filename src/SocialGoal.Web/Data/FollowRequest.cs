namespace SocialGoal.Web.Data;

/// <summary>
/// FollowRequests. Both user ids are bare nvarchar(max) columns with no FK
/// constraint in the baseline (no navigations on the legacy entity).
/// </summary>
public class FollowRequest
{
    public int FollowRequestId { get; set; }

    public required string FromUserId { get; set; }

    public required string ToUserId { get; set; }

    public bool Accepted { get; set; }
}
