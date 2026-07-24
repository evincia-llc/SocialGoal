using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): named pins for the headline enforcement DEFECTS, derived
    // live from the SocialGoal.Web assembly. Each test states the defective behavior as its name and
    // explains WHY it matters. These are characterization pins (what IS), and they become the
    // acceptance checklist for Phase 2 (policy authorization + AutoValidateAntiforgeryToken).
    // Cross-refs: LMRR R-007 (broken object-level authorization); the four sanctioned gap areas
    // (broken object-level authz, state-changing GETs / CSRF, SSRF image import, destructive
    // initializer) from the secondary risk report.
    [TestFixture]
    public class EnforcementDefectPinTests
    {
        private IReadOnlyList<ControllerSurfaceRow> _surface;

        [OneTimeSetUp]
        public void Walk()
        {
            _surface = ControllerSurfaceReflectionWalker.Walk();
        }

        // WHY: SearchController.SearchAll enumerates every goal, user, and group by free-text search
        // with NO [Authorize] anywhere on the class or method, so it is reachable anonymously while
        // every other controller requires authentication. This is the D4 anonymous-enumeration
        // evidence and the object-level-authz gap. Pin BOTH halves: the hole exists, and it is the
        // ONLY hole -- so a future stray un-authorized controller also trips this test.
        [Test]
        public void SearchController_is_the_only_controller_with_no_Authorize_gate()
        {
            var controllersWithoutAuthorize = ControllerSurfaceReflectionWalker.ControllerTypes()
                .Where(t => !t.IsDefined(typeof(System.Web.Mvc.AuthorizeAttribute), inherit: true))
                .Select(t => t.Name)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            Assert.That(controllersWithoutAuthorize, Is.EqualTo(new[] { "SearchController" }),
                "Exactly one controller (SearchController) must lack a class-level [Authorize].");

            // And SearchAll itself carries no method-level [Authorize] either.
            var searchAll = _surface.Single(r => r.Controller == "SearchController" && r.Action == "SearchAll");
            Assert.That(searchAll.ClassAuthorize, Is.False);
            Assert.That(searchAll.MethodAuthorize, Is.False);
            Assert.That(searchAll.MethodAllowAnonymous, Is.False,
                "SearchAll needs no [AllowAnonymous] -- the missing class [Authorize] already leaves it open.");
        }

        // WHY: LogOff mutates session state (signs the user out) but its [HttpPost] and
        // [ValidateAntiForgeryToken] are commented out at AccountController.cs:333-334, leaving it a
        // verb-less GET with no anti-forgery gate. A third-party page can force-logout a user via
        // <img src=".../Account/LogOff">. State-changing GET + missing CSRF gap.
        [Test]
        public void AccountController_LogOff_is_reachable_by_GET_with_no_antiforgery_token()
        {
            var logoff = _surface.Single(r => r.Controller == "AccountController" && r.Action == "LogOff");
            Assert.That(logoff.Verbs, Is.EqualTo(""), "LogOff must carry no HTTP verb attribute (commented out).");
            Assert.That(logoff.IsGetReachable, Is.True);
            Assert.That(logoff.AntiForgery, Is.False);
        }

        // WHY: GroupController.SaveUpdate creates a group update (state change) but its [HttpPost] is
        // commented out at GroupController.cs:485, so it is a verb-less GET. Unlike GoalController's
        // SaveUpdate (which keeps [HttpPost]), the group variant is CSRF-exposed and GET-mutating.
        [Test]
        public void GroupController_SaveUpdate_has_no_verb_attribute_because_HttpPost_is_commented_out()
        {
            var saveUpdate = _surface.Single(r =>
                r.Controller == "GroupController" && r.Action == "SaveUpdate");
            Assert.That(saveUpdate.Verbs, Is.EqualTo(""),
                "Group SaveUpdate's [HttpPost] is commented out -> no verb attribute present.");

            // Contrast pin: the Goal variant DOES keep [HttpPost].
            var goalSaveUpdate = _surface.Single(r =>
                r.Controller == "GoalController" && r.Action == "SaveUpdate");
            Assert.That(goalSaveUpdate.Verbs, Is.EqualTo("POST"));
        }

        // WHY: Anti-forgery protection is applied to exactly 7 POST actions, ALL on AccountController.
        // The remaining 25 POSTs (goal/group create/edit/delete, comments, invites, profile edit,
        // image upload) accept cross-site form posts. Pin the exact protected set and the exact
        // unprotected set so neither can silently change. FilterConfig registers no global
        // AutoValidateAntiforgery, so this attribute list IS the entire CSRF posture for POSTs.
        [Test]
        public void Exactly_seven_POSTs_carry_antiforgery_and_all_are_on_AccountController()
        {
            var protectedPosts = _surface.Where(r => r.IsPost && r.AntiForgery)
                .Select(r => r.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();

            var expectedProtected = new[]
            {
                "AccountController.Disassociate(String, String)",
                "AccountController.ExternalLogin(String, String)",
                "AccountController.ExternalLoginConfirmation(ExternalLoginConfirmationViewModel, String)",
                "AccountController.LinkLogin(String)",
                "AccountController.Login(LoginViewModel, String)",
                "AccountController.Manage(ManageUserViewModel)",
                "AccountController.Register(RegisterViewModel)",
            };

            Assert.That(protectedPosts, Is.EqualTo(expectedProtected));
            Assert.That(protectedPosts.All(k => k.StartsWith("AccountController.")), Is.True);
        }

        [Test]
        public void The_other_twentyfive_POSTs_carry_no_antiforgery_token()
        {
            var unprotectedPosts = _surface.Where(r => r.IsPost && !r.AntiForgery)
                .Select(r => r.Key).OrderBy(k => k, StringComparer.Ordinal).ToList();

            var expectedUnprotected = new[]
            {
                "AccountController.EditProfile(UserProfileFormModel)",
                "AccountController.UploadImage(UploadImageViewModel)",
                "GoalController.Create(GoalFormModel)",
                "GoalController.DeleteConfirmed(Int32)",
                "GoalController.DeleteConfirmedUpdate(Int32)",
                "GoalController.Edit(GoalFormModel)",
                "GoalController.EditUpdate(UpdateFormModel)",
                "GoalController.GoalStatus(Int32, Int32)",
                "GoalController.InviteEmail(InviteEmailFormModel)",
                "GoalController.InviteUser(Int32, String)",
                "GoalController.SaveComment(CommentFormModel)",
                "GoalController.SaveUpdate(UpdateFormModel)",
                "GroupController.CreateFocus(FocusFormModel)",
                "GroupController.CreateGoal(GroupGoalFormModel)",
                "GroupController.CreateGroup(GroupFormModel)",
                "GroupController.DeleteConfirmed(Int32)",
                "GroupController.DeleteConfirmedFocus(Int32)",
                "GroupController.DeleteConfirmedGroup(Int32)",
                "GroupController.DeleteConfirmedUpdate(Int32)",
                "GroupController.EditFocus(FocusFormModel)",
                "GroupController.EditGoal(GroupGoalFormModel)",
                "GroupController.EditGroup(GroupFormModel)",
                "GroupController.EditUpdate(GroupUpdateFormModel)",
                "GroupController.InviteEmail(InviteEmailFormModel)",
                "GroupController.SaveComment(GroupCommentFormModel)",
            };

            Assert.That(unprotectedPosts.Count, Is.EqualTo(25));
            Assert.That(unprotectedPosts, Is.EqualTo(expectedUnprotected));
        }

        // WHY: 23 mutating actions have no POST-family verb gate, so the default route makes them
        // GET-reachable -- every one is a CSRF target and several are BOLA targets (accept/reject
        // group requests, delete member, follow/unfollow, edit-any-profile-adjacent flows). This
        // count of 23 CORRECTS the secondary gap report's "~17": the report undercounted. The set is
        // pinned exactly. Mutates is documentation-only, so the assertion pairs it with the
        // reflection-derived IsGetReachable to prove both halves.
        [Test]
        public void Exactly_twentythree_mutating_actions_are_GET_reachable()
        {
            var expected = ExpectedControllerSurface.Rows();
            var getReachableMutations = expected
                .Where(r => r.Mutates && r.IsGetReachable)
                .Select(r => r.Key)
                .OrderBy(k => k, StringComparer.Ordinal)
                .ToList();

            var expectedSet = new[]
            {
                "AccountController.AcceptRequest(String, String)",
                "AccountController.FollowRequest(String)",
                "AccountController.LinkLoginCallback()",
                "AccountController.LogOff()",
                "AccountController.RejectRequest(String, String)",
                "AccountController.Unfollow(String)",
                "EmailRequestController.AddGroupUser()",
                "EmailRequestController.AddSupportToGoal()",
                "GoalController.SupportGoal(Int32)",
                "GoalController.SupportGoalNow(Int32)",
                "GoalController.SupportUpdate(Int32)",
                "GoalController.UnSupportGoal(Int32)",
                "GoalController.UnSupportUpdate(Int32)",
                "GroupController.AcceptRequest(Int32, String)",
                "GroupController.DeleteMember(String, Int32)",
                "GroupController.GoalStatus(Int32, Int32)",
                "GroupController.GroupJoinRequest(Int32)",
                "GroupController.InviteUser(Int32, String)",
                "GroupController.JoinGroup(Int32)",
                "GroupController.RejectRequest(Int32, String)",
                "GroupController.SaveUpdate(GroupUpdateFormModel)",
                "GroupController.SupportUpdate(Int32)",
                "GroupController.UnSupportUpdate(Int32)",
            };

            Assert.That(getReachableMutations.Count, Is.EqualTo(23));
            Assert.That(getReachableMutations, Is.EqualTo(expectedSet));

            // Prove the reflected surface agrees these 23 are genuinely verb-less / non-POST.
            var actualByKey = _surface.ToDictionary(r => r.Key, StringComparer.Ordinal);
            foreach (var key in expectedSet)
                Assert.That(actualByKey[key].IsGetReachable, Is.True,
                    key + " must be GET-reachable in the assembly.");
        }
    }
}
