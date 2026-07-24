namespace SocialGoal.Web.Data;

/// <summary>
/// GroupUsers -- group membership. Neither <see cref="GroupId"/> nor
/// <see cref="UserId"/> is constrained in the baseline: the legacy entity
/// declared no Group and no ApplicationUser navigation, so the membership
/// table that everything else hangs off has no referential integrity at all.
/// </summary>
public class GroupUser
{
    public int GroupUserId { get; set; }

    public int GroupId { get; set; }

    public required string UserId { get; set; }

    public bool Admin { get; set; }

    public DateTime AddedDate { get; set; }

    public ICollection<GroupGoal> GroupGoals { get; } = new List<GroupGoal>();
}
