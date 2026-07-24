namespace SocialGoal.Web.Data;

/// <summary>
/// RegistrationTokens. In the EF6 model (and therefore in the baseline schema)
/// despite the legacy context exposing no DbSet for it -- the registered
/// fluent configuration alone was enough to put it in the model.
/// </summary>
public class RegistrationToken
{
    public int RegistrationTokenId { get; set; }

    public Guid Token { get; set; }

    public string? Role { get; set; }
}
