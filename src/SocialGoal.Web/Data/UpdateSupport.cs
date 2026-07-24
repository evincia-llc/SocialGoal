namespace SocialGoal.Web.Data;

/// <summary>UpdateSupports.</summary>
public class UpdateSupport
{
    public int UpdateSupportId { get; set; }

    public int UpdateId { get; set; }

    public string? UserId { get; set; }

    public DateTime UpdateSupportedDate { get; set; }

    public Update? Update { get; set; }
}
