namespace SocialGoal.Web.Data;

/// <summary>
/// SecurityTokens -- convention-only in EF6 (no registered configuration), so
/// every facet here is a plain default.
/// </summary>
public class SecurityToken
{
    public int SecurityTokenId { get; set; }

    public Guid Token { get; set; }

    public int ActualID { get; set; }
}
