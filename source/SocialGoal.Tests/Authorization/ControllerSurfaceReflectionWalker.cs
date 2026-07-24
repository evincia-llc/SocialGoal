using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): derives the ACTUAL declarative enforcement surface from
    // the compiled SocialGoal.Web assembly by reflection, so the pinned expected table can be diffed
    // against reality. Mirrors the action-method selection rules ASP.NET MVC 5 itself applies
    // (System.Web.Mvc.ReflectedControllerDescriptor / ActionMethodSelector.IsValidActionMethod):
    //   - public, instance, declared on the controller (inherited Controller/Object members excluded);
    //   - not special-name (property/event/operator accessors excluded);
    //   - not [NonAction];
    //   - not a generic method definition.
    // Non-ActionResult return types (void, int, IEnumerable<T>, JsonResult, ...) still count -- MVC
    // treats every qualifying public method as an action. That is deliberate: the anonymous
    // enumeration surface includes those helper-typed actions, so the pin must include them too.
    public static class ControllerSurfaceReflectionWalker
    {
        // A type in SocialGoal.Web anchors the assembly under test. SearchController is chosen
        // precisely because it is the one controller with no [Authorize] -- if it were ever removed
        // the anchor would fail loudly rather than silently drifting.
        public static Assembly WebAssembly
        {
            get { return typeof(SocialGoal.Web.Controllers.SearchController).Assembly; }
        }

        public static IReadOnlyList<Type> ControllerTypes()
        {
            return WebAssembly.GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && typeof(Controller).IsAssignableFrom(t))
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();
        }

        public static IReadOnlyList<ControllerSurfaceRow> Walk()
        {
            var rows = new List<ControllerSurfaceRow>();
            foreach (var controller in ControllerTypes())
            {
                bool classAuthorize = controller.IsDefined(typeof(AuthorizeAttribute), inherit: true);
                bool classAllowAnonymous = controller.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);

                foreach (var method in ActionMethods(controller))
                {
                    rows.Add(new ControllerSurfaceRow
                    {
                        Controller = controller.Name,
                        Action = method.Name,
                        Signature = SignatureOf(method),
                        Verbs = VerbsOf(method),
                        ActionName = ActionNameOf(method),
                        ClassAuthorize = classAuthorize,
                        ClassAllowAnonymous = classAllowAnonymous,
                        MethodAuthorize = method.IsDefined(typeof(AuthorizeAttribute), inherit: true),
                        MethodAllowAnonymous = method.IsDefined(typeof(AllowAnonymousAttribute), inherit: true),
                        AntiForgery = method.IsDefined(typeof(ValidateAntiForgeryTokenAttribute), inherit: true),
                        ChildActionOnly = method.IsDefined(typeof(ChildActionOnlyAttribute), inherit: true)
                    });
                }
            }
            return rows
                .OrderBy(r => r.Controller, StringComparer.Ordinal)
                .ThenBy(r => r.Action, StringComparer.Ordinal)
                .ThenBy(r => r.Signature, StringComparer.Ordinal)
                .ToList();
        }

        private static IEnumerable<MethodInfo> ActionMethods(Type controller)
        {
            return controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(IsActionMethod);
        }

        private static bool IsActionMethod(MethodInfo method)
        {
            if (method.IsSpecialName) return false;                       // property/event/operator accessor
            if (method.IsGenericMethodDefinition) return false;
            if (method.IsDefined(typeof(NonActionAttribute), inherit: true)) return false;

            // Exclude anything whose base definition lives on Controller/Object (an overridden
            // framework method is never an action even though DeclaredOnly would surface it).
            var declaring = method.GetBaseDefinition().DeclaringType;
            if (declaring == typeof(object) || declaring == typeof(Controller)) return false;

            return true;
        }

        private static string SignatureOf(MethodInfo method)
        {
            var names = method.GetParameters().Select(p => p.ParameterType.Name);
            return "(" + string.Join(", ", names) + ")";
        }

        private static string VerbsOf(MethodInfo method)
        {
            var verbs = new List<string>();
            if (method.IsDefined(typeof(HttpGetAttribute), true)) verbs.Add("GET");
            if (method.IsDefined(typeof(HttpPostAttribute), true)) verbs.Add("POST");
            if (method.IsDefined(typeof(HttpPutAttribute), true)) verbs.Add("PUT");
            if (method.IsDefined(typeof(HttpDeleteAttribute), true)) verbs.Add("DELETE");
            if (method.IsDefined(typeof(HttpPatchAttribute), true)) verbs.Add("PATCH");
            return string.Join(",", verbs);
        }

        private static string ActionNameOf(MethodInfo method)
        {
            var attr = (ActionNameAttribute)method
                .GetCustomAttributes(typeof(ActionNameAttribute), inherit: true)
                .FirstOrDefault();
            return attr == null ? null : attr.Name;
        }
    }
}
