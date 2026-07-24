using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Spikes;

/// <summary>
/// Sprint 5 gating spike (epic: "Identity spike"). Proves the Identity 1.0
/// password-hash format verifies under ASP.NET Core Identity's compatibility
/// path and is transparently rehashed on successful login, against the
/// legacy-shaped AspNetUsers table (string IDs, baseline schema).
/// Green here is the Sprint 8 auth re-platform's entry evidence.
/// </summary>
[TestFixture]
[NonParallelizable]
public class IdentityPasswordCompatSpikeTests
{
    private const string SpikeDb = "SocialGoal_ModernSpike_Identity";

    /// <summary>
    /// Produced by the REAL legacy stack, not a reimplementation:
    /// Microsoft.AspNet.Identity.Core 1.0.0 (net45, the exact package the
    /// legacy app pins) loaded into Windows PowerShell 5.1 (.NET Framework
    /// 4.8 CLR), `(New-Object Microsoft.AspNet.Identity.PasswordHasher).
    /// HashPassword('Test@1234')`, 2026-07-24. The legacy hasher verified it
    /// (Success); decoded: 49 bytes, format marker 0x00 = ASP.NET Identity v2
    /// format (PBKDF2-HMAC-SHA1, 128-bit salt, 1000 iterations).
    /// </summary>
    private const string LegacyHash = "AOErQpdoxwr0++/XpYWDTw0O40J77qfWHrC0DGuPu5AudixaRwMxb1k0N9FjGAbxHQ==";
    private const string LegacyPassword = "Test@1234";
    private const string LegacyUserId = "spike-user-0001";

    [OneTimeSetUp]
    public void CreateLegacyShapedDatabase()
    {
        LocalDb.CreateDatabase(SpikeDb);
        LocalDb.ExecuteScript(SpikeDb, File.ReadAllText(LocalDb.SchemaBaselinePath()));

        using var context = NewContext();
        context.Users.Add(new ApplicationUser
        {
            Id = LegacyUserId,
            UserName = "spikeuser",
            PasswordHash = LegacyHash,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            DateCreated = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        });
        context.SaveChanges();
    }

    [OneTimeTearDown]
    public void DropDatabase() => LocalDb.DropDatabase(SpikeDb);

    [Test]
    public void LegacyHash_VerifiesUnderCoreIdentity_AndReportsRehashNeeded()
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        var user = new ApplicationUser { Id = LegacyUserId, UserName = "spikeuser" };

        var result = hasher.VerifyHashedPassword(user, LegacyHash, LegacyPassword);

        // SuccessRehashNeeded is the compatibility contract: the v2-format
        // hash verifies, and Core Identity asks for an upgrade to v3.
        Assert.That(result, Is.EqualTo(PasswordVerificationResult.SuccessRehashNeeded));
    }

    [Test]
    public void LegacyHash_RejectsWrongPassword()
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        var user = new ApplicationUser { Id = LegacyUserId, UserName = "spikeuser" };

        var result = hasher.VerifyHashedPassword(user, LegacyHash, "WrongPassword!");

        Assert.That(result, Is.EqualTo(PasswordVerificationResult.Failed));
    }

    [Test]
    public async Task CheckPassword_SucceedsAndRehashesToV3_InTheLegacyShapedTable()
    {
        using (var context = NewContext())
        {
            var manager = NewUserManager(context);
            var user = await context.Users.SingleAsync(u => u.Id == LegacyUserId);

            Assert.That(await manager.CheckPasswordAsync(user, "WrongPassword!"), Is.False,
                "Wrong password must fail through the UserManager path too.");
            Assert.That(await manager.CheckPasswordAsync(user, LegacyPassword), Is.True,
                "The legacy v2 hash must authenticate through UserManager.");
        }

        // Fresh context: what actually got persisted to AspNetUsers?
        using (var context = NewContext())
        {
            var user = await context.Users.SingleAsync(u => u.Id == LegacyUserId);

            Assert.That(user.PasswordHash, Is.Not.Null.And.Not.EqualTo(LegacyHash),
                "Successful login must rehash: the stored hash changes.");
            Assert.That(Convert.FromBase64String(user.PasswordHash!)[0], Is.EqualTo(0x01),
                "The rewritten hash carries the v3 format marker.");

            var hasher = new PasswordHasher<ApplicationUser>();
            Assert.That(hasher.VerifyHashedPassword(user, user.PasswordHash!, LegacyPassword),
                Is.EqualTo(PasswordVerificationResult.Success),
                "The upgraded hash verifies as plain (no further rehash) v3.");

            var manager = NewUserManager(context);
            Assert.That(await manager.CheckPasswordAsync(user, LegacyPassword), Is.True,
                "The user still authenticates after the upgrade.");
        }
    }

    private static SocialGoalDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SocialGoalDbContext>()
            .UseSqlServer(LocalDb.ConnectionStringFor(SpikeDb))
            .Options);

    private static UserManager<ApplicationUser> NewUserManager(SocialGoalDbContext context) =>
        new(new LegacyShapedUserStore(context),
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            userValidators: [],
            passwordValidators: [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
}
