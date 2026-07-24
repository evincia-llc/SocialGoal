namespace SocialGoal.Web.Data;

/// <summary>
/// FollowUsers. Four FK columns into AspNetUsers: the two real ones behind
/// <see cref="FromUser"/>/<see cref="ToUser"/>, plus the pair of EF6 shadow
/// columns produced by <see cref="ApplicationUser.FollowFromUser"/> and
/// <see cref="ApplicationUser.FollowToUser"/> never pairing with them.
/// </summary>
public class FollowUser
{
    public int FollowUserId { get; set; }

    public string? ToUserId { get; set; }

    public string? FromUserId { get; set; }

    public bool Accepted { get; set; }

    public DateTime AddedDate { get; set; }

    public ApplicationUser? ToUser { get; set; }

    public ApplicationUser? FromUser { get; set; }
}
