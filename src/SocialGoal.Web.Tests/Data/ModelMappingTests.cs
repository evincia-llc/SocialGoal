using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;

namespace SocialGoal.Web.Tests.Data;

/// <summary>
/// The EF Core port of the Sprint 2 MappingSmokeTests
/// (source/SocialGoal.Tests/Data/MappingSmokeTests.cs): what the model
/// SocialGoalDbContext.OnModelCreating actually produces, read from metadata,
/// with no database involved. Pins the real generated names -- including the
/// ones EF6 arrived at by pluralization and EF Core only has because they are
/// stated outright -- rather than what the configuration classes appear to
/// intend.
///
/// Physical schema parity is NOT this fixture's job: SchemaParityTests compares
/// the live catalog against docs/schema/schema-baseline.sql and
/// MigrationModelDriftTests pins model-to-migration coherence, which together
/// carry everything the legacy SchemaSnapshotTests guaranteed. This fixture
/// covers the model-level claims those two cannot see.
/// </summary>
[TestFixture]
public class ModelMappingTests
{
    /// <summary>
    /// CLR entity type -> schema.table for the whole model. Thirty entries: the
    /// legacy snapshot's thirty entity sets, minus IdentityUser (EF6's TPH base,
    /// which ApplicationUser shared) plus ApplicationUser in its own right.
    /// Dead GoalUpdate never enters, on either side.
    /// </summary>
    private static readonly SortedDictionary<string, string> ExpectedModel =
        new(StringComparer.Ordinal)
        {
            { "ApplicationUser", "dbo.AspNetUsers" },
            { "Comment", "dbo.Comments" },
            { "CommentUser", "dbo.CommentUsers" },
            { "Focus", "dbo.Foci" },
            { "FollowRequest", "dbo.FollowRequests" },
            { "FollowUser", "dbo.FollowUsers" },
            { "Goal", "dbo.Goals" },
            { "GoalStatus", "dbo.GoalStatus" },
            { "Group", "dbo.Groups" },
            { "GroupComment", "dbo.GroupComments" },
            { "GroupCommentUser", "dbo.GroupCommentUsers" },
            { "GroupGoal", "dbo.GroupGoals" },
            { "GroupInvitation", "dbo.GroupInvitations" },
            { "GroupRequest", "dbo.GroupRequests" },
            { "GroupUpdate", "dbo.GroupUpdates" },
            { "GroupUpdateSupport", "dbo.GroupUpdateSupports" },
            { "GroupUpdateUser", "dbo.GroupUpdateUsers" },
            { "GroupUser", "dbo.GroupUsers" },
            { "IdentityRole", "dbo.AspNetRoles" },
            { "IdentityUserClaim", "dbo.AspNetUserClaims" },
            { "IdentityUserLogin", "dbo.AspNetUserLogins" },
            { "IdentityUserRole", "dbo.AspNetUserRoles" },
            { "Metric", "dbo.Metrics" },
            { "RegistrationToken", "dbo.RegistrationTokens" },
            { "SecurityToken", "dbo.SecurityTokens" },
            { "Support", "dbo.Supports" },
            { "SupportInvitation", "dbo.SupportInvitations" },
            { "Update", "dbo.Updates" },
            { "UpdateSupport", "dbo.UpdateSupports" },
            { "UserProfile", "dbo.UserProfiles" },
        };

    // Never opened: every assertion here reads model metadata.
    private static SocialGoalDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SocialGoalDbContext>()
            .UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SocialGoal_NeverOpened")
            .Options);

    private static SortedDictionary<string, string> ActualModel(SocialGoalDbContext context)
    {
        var map = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            map[entityType.ClrType.Name] = $"{entityType.GetSchema() ?? "dbo"}.{entityType.GetTableName()}";
        }

        return map;
    }

    [Test]
    public void GeneratedModel_MatchesCommittedSnapshot()
    {
        using var context = NewContext();

        Assert.That(ActualModel(context), Is.EqualTo(ExpectedModel),
            "The entity-type/table model drifted from the committed snapshot.");
    }

    [Test]
    public void DeadApplicationUserConfiguration_IsNotHonored_FirstNameStaysNullableAndUncapped()
    {
        // The legacy solution carries an ApplicationUserConfiguration that would
        // make FirstName required with MaxLength(1). It was never registered, so
        // it never applied, and the baseline schema has FirstName as a nullable
        // nvarchar(max). Porting it "for completeness" would truncate every
        // first name in the database to one character -- this test is here to
        // stop that from ever looking like a tidy-up.
        using var context = NewContext();
        var firstName = context.Model.FindEntityType(typeof(ApplicationUser))!
            .FindProperty(nameof(ApplicationUser.FirstName))!;

        Assert.Multiple(() =>
        {
            Assert.That(firstName.IsNullable, Is.True,
                "FirstName is required, which means the dead configuration's IsRequired() was ported.");
            Assert.That(firstName.GetMaxLength(), Is.Null,
                "FirstName has a max length, which means the dead configuration's HasMaxLength(1) was ported.");
        });
    }

    [Test]
    public void RegistrationToken_IsInTheModel_MappedToRegistrationTokens()
    {
        // Legacy had no DbSet<RegistrationToken>; its registered configuration
        // alone put it in the EF6 model and therefore in the baseline schema.
        // The port keeps the entity and adds the DbSet the legacy context
        // lacked -- the mapping is what matters, and it must not be dropped on
        // the grounds that "nothing uses it".
        using var context = NewContext();
        var entityType = context.Model.FindEntityType(typeof(RegistrationToken));

        Assert.That(entityType, Is.Not.Null);
        Assert.That(entityType!.GetTableName(), Is.EqualTo("RegistrationTokens"));
    }

    [Test]
    public void UserProfileUserId_IsFiftyCharacters_ReproducingTheLegacyUndersizing()
    {
        // A faithfully reproduced defect, not an oversight: UserProfiles.UserId
        // stores an Identity id, which is nvarchar(128) everywhere else in the
        // schema, but the legacy configuration capped it at 50 -- and there is
        // no FK. Recorded in ai-context/lmrr-feedback.md; correcting it is a
        // schema change that needs its own decision, so this test makes a
        // silent "fix" impossible.
        using var context = NewContext();
        var userId = context.Model.FindEntityType(typeof(UserProfile))!
            .FindProperty(nameof(UserProfile.UserId))!;

        Assert.That(userId.GetMaxLength(), Is.EqualTo(50));
    }
}
