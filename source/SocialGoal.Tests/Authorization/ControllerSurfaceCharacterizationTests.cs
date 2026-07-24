using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): pins the full declarative enforcement surface of
    // SocialGoal.Web by diffing the reflection-derived surface against the hand-maintained expected
    // table (ExpectedControllerSurface). Characterization only -- these assertions encode what the
    // legacy app declares TODAY (R-007 and the CSRF/mutating-GET gap areas), so any attribute drift
    // fails as a single readable row and forces a conscious update to the pin.
    [TestFixture]
    public class ControllerSurfaceCharacterizationTests
    {
        private IReadOnlyList<ControllerSurfaceRow> _actual;
        private IReadOnlyList<ControllerSurfaceRow> _expected;

        [OneTimeSetUp]
        public void Walk()
        {
            _actual = ControllerSurfaceReflectionWalker.Walk();
            _expected = ExpectedControllerSurface.Rows();
        }

        [Test]
        public void The_reflected_surface_matches_the_pinned_surface_row_for_row()
        {
            var expectedByKey = _expected.ToDictionary(r => r.Key, StringComparer.Ordinal);
            var actualByKey = _actual.ToDictionary(r => r.Key, StringComparer.Ordinal);

            var problems = new List<string>();

            foreach (var key in actualByKey.Keys.Where(k => !expectedByKey.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal))
                problems.Add("UNPINNED action present in assembly but not in expected table: " + key
                             + "  [" + actualByKey[key].DescribeDeclarative() + "]");

            foreach (var key in expectedByKey.Keys.Where(k => !actualByKey.ContainsKey(k)).OrderBy(k => k, StringComparer.Ordinal))
                problems.Add("MISSING action pinned in expected table but absent from assembly: " + key);

            foreach (var key in expectedByKey.Keys.Where(actualByKey.ContainsKey).OrderBy(k => k, StringComparer.Ordinal))
            {
                var exp = expectedByKey[key];
                var act = actualByKey[key];
                if (exp.DescribeDeclarative() != act.DescribeDeclarative())
                    problems.Add(key + Environment.NewLine
                        + "    expected: " + exp.DescribeDeclarative() + Environment.NewLine
                        + "    but was:  " + act.DescribeDeclarative());
            }

            if (problems.Count > 0)
                Assert.Fail("Declarative enforcement surface drifted from the pin (" + problems.Count
                    + " row(s)):" + Environment.NewLine + string.Join(Environment.NewLine, problems));
        }

        [Test]
        public void The_assembly_exposes_exactly_149_public_actions()
        {
            // Total action count is itself a pinned fact: a new action (or a helper method made
            // public) must be added to the table deliberately, not slip in unpinned.
            Assert.That(_actual.Count, Is.EqualTo(149));
        }

        [Test]
        public void Each_controller_exposes_its_pinned_action_count()
        {
            // Per-controller counts localize a drift to one controller before the row diff runs.
            var expectedCounts = new Dictionary<string, int>
            {
                { "AccountController", 30 },
                { "EmailRequestController", 2 },
                { "GoalController", 43 },
                { "GroupController", 64 },
                { "HomeController", 6 },
                { "NotificationController", 3 },
                { "SearchController", 1 },
            };

            var actualCounts = _actual.GroupBy(r => r.Controller)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            CollectionAssert.AreEquivalent(expectedCounts.Keys, actualCounts.Keys,
                "The set of controller types changed.");
            foreach (var kv in expectedCounts)
                Assert.That(actualCounts[kv.Key], Is.EqualTo(kv.Value),
                    kv.Key + " action count drifted from the pin.");
        }

        [Test]
        public void Action_keys_are_unique_so_every_overload_is_pinned_distinctly()
        {
            // The (controller, action, signature) key must be 1:1 with a method. If two rows collapse
            // to one key the surface would silently under-pin an overload (e.g. a GET/POST pair).
            var dupExpected = _expected.GroupBy(r => r.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            var dupActual = _actual.GroupBy(r => r.Key).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Assert.That(dupExpected, Is.Empty, "Duplicate keys in expected table: " + string.Join(", ", dupExpected));
            Assert.That(dupActual, Is.Empty, "Duplicate keys in reflected surface: " + string.Join(", ", dupActual));
        }

        [Test]
        public void Every_mutating_action_key_resolves_to_exactly_one_pinned_action()
        {
            // Mutates is documentation-only data: prove each declared mutating key names a real,
            // unique action in the surface (existence + uniqueness), since reflection cannot derive it.
            var byKey = _actual.ToDictionary(r => r.Key, StringComparer.Ordinal);
            var unresolved = ExpectedControllerSurface.MutatingActionKeys()
                .Where(k => !byKey.ContainsKey(k))
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToList();
            Assert.That(unresolved, Is.Empty,
                "Mutating keys that do not match any action in the assembly: " + Environment.NewLine
                + string.Join(Environment.NewLine, unresolved));
        }

        [Test]
        public void The_walker_excludes_non_action_members()
        {
            // Proves the walker's action-selection rules: non-public helpers, the protected Dispose
            // override, and property accessors on AccountController are NOT surfaced as actions.
            var accountActions = _actual.Where(r => r.Controller == "AccountController")
                .Select(r => r.Action).ToList();

            foreach (var nonAction in new[]
            {
                "GetImageFromUrl",   // private helper (the SSRF fetch) -- not public, must be excluded
                "GetUrlFileName",    // private helper
                "CreateImage",       // private helper
                "GetErrorsFromModelState", // private helper
                "SignInAsync",       // private
                "HasPassword",       // private
                "Dispose",           // protected override of Controller.Dispose
                "AuthenticationManager", // property -> get_/set_ accessors are special-name
                "get_AuthenticationManager",
                "set_AuthenticationManager",
            })
            {
                Assert.That(accountActions, Has.No.Member(nonAction),
                    "'" + nonAction + "' must not be treated as a controller action.");
            }
        }
    }
}
