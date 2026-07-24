namespace SocialGoal.Web.Data;

/// <summary>
/// UserProfiles. Unconstrained and mis-sized: <see cref="UserId"/> is
/// nvarchar(50) in the baseline while the Identity id it stores is
/// nvarchar(128), and there is no FK. Reproduced faithfully -- correcting it
/// is a schema change, not a port.
/// </summary>
public class UserProfile
{
    public int UserProfileId { get; set; }

    public DateTime DateEdited { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool? Gender { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public double? ZipCode { get; set; }

    public double? ContactNo { get; set; }

    public string? UserId { get; set; }
}
