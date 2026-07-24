using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Builds a <see cref="ControllerContext"/> whose principal is an authenticated
    /// <see cref="ClaimsPrincipal"/> carrying a NameIdentifier claim. The legacy
    /// controllers read the acting user via <c>User.Identity.GetUserId()</c>
    /// (Microsoft.AspNet.Identity), which resolves to
    /// <c>ClaimsIdentity.FindFirstValue(ClaimTypes.NameIdentifier)</c> -- so this
    /// is exactly the identity surface the actions depend on.
    ///
    /// Note (harness design, "Wiring facts"): [Authorize] is NOT executed by direct
    /// action invocation, so "anonymous can reach a mutation" is already pinned by
    /// the surface tests. The behavioral matrix drives the AUTHENTICATED-but-
    /// unauthorized actors (unrelated user, non-admin member, non-recipient) where
    /// the broken-object-level-authorization defect (R-007) actually lives.
    /// </summary>
    internal static class AuthenticatedActor
    {
        internal const string TestAuthenticationType = "TestAuth";

        /// <summary>
        /// Signs <paramref name="controller"/> in as <paramref name="userId"/>.
        /// Pass <c>null</c> for an anonymous (unauthenticated) principal.
        /// </summary>
        internal static void SignIn(Controller controller, string userId)
        {
            ClaimsIdentity identity = userId == null
                ? new ClaimsIdentity() // IsAuthenticated == false
                : new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
                    TestAuthenticationType);

            var principal = new ClaimsPrincipal(identity);

            var httpContext = new Mock<HttpContextBase>();
            httpContext.SetupGet(c => c.User).Returns(principal);

            controller.ControllerContext =
                new ControllerContext(httpContext.Object, new RouteData(), controller);
            controller.TempData = new TempDataDictionary();
        }
    }
}
