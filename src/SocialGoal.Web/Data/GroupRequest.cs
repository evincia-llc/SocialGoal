namespace SocialGoal.Web.Data;

/// <summary>
/// GroupRequests -- the one "user id" column in the non-Goal half of the model
/// that EF6 did constrain, because the legacy entity carried both a
/// <see cref="User"/> navigation and the matching
/// <see cref="ApplicationUser.GroupRequests"/> collection.
/// </summary>
public class GroupRequest
{
    public int GroupRequestId { get; set; }

    public int GroupId { get; set; }

    public string? UserId { get; set; }

    public bool Accepted { get; set; }

    public Group? Group { get; set; }

    public ApplicationUser? User { get; set; }
}
