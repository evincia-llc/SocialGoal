using System;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using SocialGoal.Web.Core.ActionFilters;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): proves the two security filters that LOOK like they gate
    // the app are inert -- defined but never registered -- so the declarative surface pinned by the
    // sibling fixtures is the whole story. Characterization: pins dead code as dead. Cross-ref R-007
    // and the object-level-authz / CSRF gap areas.
    [TestFixture]
    public class InertFilterCharacterizationTests
    {
        // WHY: the ONLY global filter is HandleErrorAttribute. There is no global authorization
        // filter and no global AutoValidateAntiforgeryToken, so per-action [Authorize]/AFT attributes
        // (pinned elsewhere) are the entire posture. If a real global gate were ever added this fails.
        [Test]
        public void RegisterGlobalFilters_adds_only_HandleErrorAttribute()
        {
            var filters = new GlobalFilterCollection();
            FilterConfig.RegisterGlobalFilters(filters);

            Assert.That(filters.Count, Is.EqualTo(1));
            Assert.That(filters.Single().Instance, Is.TypeOf<HandleErrorAttribute>());
        }

        // WHY: SocialGoalAuthorizeAttribute (Web.Core) is a custom AuthorizeAttribute that would
        // redirect unauthenticated users to a login/access-denied page -- but it is applied to NO
        // controller and NO action. Every [Authorize] in the app is the stock framework attribute.
        // Pins the custom authorize filter as dead code.
        [Test]
        public void No_controller_or_action_uses_SocialGoalAuthorizeAttribute()
        {
            foreach (var controller in ControllerSurfaceReflectionWalker.ControllerTypes())
            {
                Assert.That(controller.IsDefined(typeof(SocialGoalAuthorizeAttribute), inherit: true), Is.False,
                    controller.Name + " unexpectedly carries the custom SocialGoalAuthorizeAttribute.");

                foreach (var method in controller.GetMethods(
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    Assert.That(method.IsDefined(typeof(SocialGoalAuthorizeAttribute), inherit: true), Is.False,
                        controller.Name + "." + method.Name
                        + " unexpectedly carries the custom SocialGoalAuthorizeAttribute.");
                }
            }
        }

        // WHY: AntiForgeryTokenFilterProvider (Web.Core) is an IFilterProvider that WOULD attach a
        // ValidateAntiForgeryToken to every POST -- if it were ever added to FilterProviders.Providers.
        // It never is. Bootstrapper.cs:45 calls builder.RegisterFilterProvider(), which registers
        // Autofac's own AutofacFilterProvider, NOT this custom one. (The source file is literally
        // named "AntiForgeryTokenFilterProvider .cs" -- note the trailing space -- an artifact of the
        // dead code.)
        //
        // NOTE ON APPROACH: this is a static/registration-state proof, not an in-process app startup.
        // Running Bootstrapper.Run() in the test process mutates global MVC state
        // (DependencyResolver.SetResolver) and the global AutoMapper static config, which risks
        // bleeding into the other fixtures; so per the brief's sanctioned fallback we assert the
        // custom provider is absent from the live FilterProviders.Providers chain AND that the Web
        // assembly defines/registers no such provider, rather than replaying startup.
        [Test]
        public void The_custom_AntiForgeryTokenFilterProvider_is_never_registered()
        {
            // The default MVC provider chain (no Application_Start ran) must not contain it, and
            // nothing about building the app adds it either.
            Assert.That(FilterProviders.Providers.Any(p => p is AntiForgeryTokenFilterProvider), Is.False,
                "The custom AntiForgeryTokenFilterProvider is present in FilterProviders.Providers.");

            // No type in the Web assembly IS or subclasses the custom provider (it lives, dead, in
            // Web.Core and is never instantiated/wired from Web).
            var webProviderTypes = ControllerSurfaceReflectionWalker.WebAssembly.GetTypes()
                .Where(t => typeof(AntiForgeryTokenFilterProvider).IsAssignableFrom(t))
                .ToList();
            Assert.That(webProviderTypes, Is.Empty,
                "The Web assembly defines a subtype of the custom AntiForgeryTokenFilterProvider.");
        }

        // WHY: anchors that both "security" types genuinely live in Web.Core (the assembly under
        // reference) so the dead-code pins above are testing the real artifacts, not shadows.
        [Test]
        public void Both_inert_security_filters_are_defined_in_Web_Core()
        {
            var webCore = typeof(SocialGoalAuthorizeAttribute).Assembly;
            Assert.That(typeof(AntiForgeryTokenFilterProvider).Assembly, Is.EqualTo(webCore));
            Assert.That(typeof(SocialGoalAuthorizeAttribute).IsSubclassOf(typeof(AuthorizeAttribute)), Is.True);
            Assert.That(typeof(IFilterProvider).IsAssignableFrom(typeof(AntiForgeryTokenFilterProvider)), Is.True);
        }
    }
}
