using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;

namespace SocialGoal.Web.Tests.Spikes;

/// <summary>
/// Spike-only minimal user store: just enough of IUserStore for
/// UserManager.CheckPasswordAsync's verify + rehash-on-login path, backed by
/// the legacy-shaped AspNetUsers table via SocialGoalDbContext. The real
/// Sprint 8 store replaces this; it exists so the hash-compat spike exercises
/// UserManager against the actual legacy schema rather than the hasher alone.
/// </summary>
internal sealed class LegacyShapedUserStore(SocialGoalDbContext context) :
    IUserPasswordStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>
{
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id);

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.UserName);

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        return Task.CompletedTask;
    }

    // The legacy AspNetUsers table has no NormalizedUserName column; the spike
    // surfaces UserName itself. Sprint 8's reviewed migration adds the real one.
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.UserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return IdentityResult.Success;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
        await context.Users.FindAsync([userId], cancellationToken).ConfigureAwait(false);

    public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        context.Users.FirstOrDefaultAsync(u => u.UserName == normalizedUserName, cancellationToken);

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.PasswordHash is not null);

    public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        Task.FromResult(user.SecurityStamp);

    public void Dispose()
    {
        // Context lifetime is owned by the test fixture.
    }
}
