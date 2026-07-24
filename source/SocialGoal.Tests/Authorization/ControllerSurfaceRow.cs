using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): one row per public controller action, describing the
    // *declarative* enforcement surface -- the attributes ASP.NET MVC actually reads to gate a
    // request (verb, [Authorize]/[AllowAnonymous], [ValidateAntiForgeryToken], [ChildActionOnly]).
    //
    // This is a CHARACTERIZATION model: it pins what the legacy code declares today, defects
    // included. It is not an aspiration. Every field except Mutates is derived by reflection from
    // the SocialGoal.Web assembly (see ControllerSurfaceReflectionWalker). Mutates is the one field
    // reflection cannot know -- it comes from the human controller-surface inventory and rides along
    // as documentation so the table doubles as the Phase 2 enforcement spec (R-007: broken
    // object-level authorization; the four gap areas: state-changing GETs / CSRF).
    public sealed class ControllerSurfaceRow
    {
        // Simple type name, e.g. "AccountController".
        public string Controller { get; set; }

        // Reflection method name (before any [ActionName] rename), e.g. "LogOff".
        public string Action { get; set; }

        // Parameter-type signature, e.g. "(LoginViewModel, String)". Disambiguates action
        // overloads (GET form + POST handler share a method name).
        public string Signature { get; set; }

        // Normalized HTTP verb attributes present on the method: "GET", "POST", "GET,POST", or ""
        // (no verb attribute -> reachable by any verb, GET included, under the default route).
        public string Verbs { get; set; }

        // [ActionName("X")] override, or null when the route name equals the method name.
        public string ActionName { get; set; }

        // [Authorize] on the declaring controller type.
        public bool ClassAuthorize { get; set; }

        // [AllowAnonymous] on the declaring controller type (none exist today; pinned as false).
        public bool ClassAllowAnonymous { get; set; }

        // [Authorize] on the method itself.
        public bool MethodAuthorize { get; set; }

        // [AllowAnonymous] on the method itself.
        public bool MethodAllowAnonymous { get; set; }

        // [ValidateAntiForgeryToken] on the method.
        public bool AntiForgery { get; set; }

        // [ChildActionOnly] on the method.
        public bool ChildActionOnly { get; set; }

        // DOCUMENTATION-ONLY, not reflection-derivable. Carried from the controller-surface
        // inventory: does the action change server-visible state? Asserted for existence and
        // uniqueness against the reflected surface, never derived from it.
        public bool Mutates { get; set; }

        // Stable identity of an action across the expected table and the reflected surface.
        public string Key
        {
            get { return Controller + "." + Action + Signature; }
        }

        // True when no POST-family verb gates the action, so the default route makes it
        // GET-reachable. "" (no verb attribute) and a bare [HttpGet] both qualify.
        public bool IsGetReachable
        {
            get
            {
                var verbs = SplitVerbs(Verbs);
                return !verbs.Contains("POST") && !verbs.Contains("PUT")
                    && !verbs.Contains("DELETE") && !verbs.Contains("PATCH");
            }
        }

        public bool IsPost
        {
            get { return SplitVerbs(Verbs).Contains("POST"); }
        }

        private static IEnumerable<string> SplitVerbs(string verbs)
        {
            if (string.IsNullOrEmpty(verbs))
                return Enumerable.Empty<string>();
            return verbs.Split(',');
        }

        // The reflection-derivable columns, compared field-by-field in the surface diff so a change
        // to any one controller attribute fails exactly one readable row.
        public string DescribeDeclarative()
        {
            return string.Format(
                "verbs=[{0}] actionName=[{1}] classAuthorize={2} classAllowAnonymous={3} " +
                "methodAuthorize={4} methodAllowAnonymous={5} antiForgery={6} childActionOnly={7}",
                Verbs, ActionName ?? "(none)", ClassAuthorize, ClassAllowAnonymous,
                MethodAuthorize, MethodAllowAnonymous, AntiForgery, ChildActionOnly);
        }

        public override string ToString()
        {
            return Key + " { " + DescribeDeclarative() + " mutates=" + Mutates + " }";
        }
    }
}
