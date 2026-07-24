namespace SocialGoal.Web.Data;

/// <summary>
/// GroupUpdateSupports. <see cref="GroupUserId"/> has no FK constraint in the
/// baseline -- the legacy entity declared no GroupUser navigation.
/// </summary>
public class GroupUpdateSupport
{
    public int GroupUpdateSupportId { get; set; }

    public int GroupUpdateId { get; set; }

    public int GroupUserId { get; set; }

    public DateTime UpdateSupportedDate { get; set; }

    public GroupUpdate? GroupUpdate { get; set; }
}
