using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialGoal.Tests.Authorization
{
    // Sprint 3 matrix spec (Workstream B): the single source-of-truth pinned enforcement surface for
    // SocialGoal.Web, one row per public controller action (149 total). This table is a
    // CHARACTERIZATION snapshot -- it records the declarative gate each action carries TODAY, defects
    // and all. It was seeded from a reflection walk of the shipped assembly and then verified
    // line-by-line against controller source; from here on it is hand-maintained, so any drift in a
    // controller attribute produces a one-row diff against ControllerSurfaceReflectionWalker.Walk().
    //
    // It is the Phase 2 enforcement spec: when policy-based authorization and AutoValidateAntiforgery
    // replace this surface, these rows are the checklist of what must be covered (R-007 broken
    // object-level authorization; the sanctioned gap areas: state-changing GETs / CSRF).
    //
    // Column provenance:
    //   - verbs / actionName / class+method [Authorize]/[AllowAnonymous] / AFT / [ChildActionOnly]
    //     are reflection-derivable and diffed against the live surface.
    //   - ClassAuthorize is pinned per controller: every controller carries [Authorize] EXCEPT
    //     SearchController (the anonymous enumeration hole -- see EnforcementDefectPinTests).
    //   - Mutates is DOCUMENTATION-ONLY (reflection cannot know it). It follows the controller-surface
    //     inventory's state-change classification and is asserted for existence/uniqueness only.
    public static class ExpectedControllerSurface
    {
        // Mutating actions per the inventory (53 total = 30 mutating POSTs + 23 mutating GETs).
        // The two POSTs that are NOT mutating -- AccountController.ExternalLogin and .LinkLogin --
        // only return a ChallengeResult (redirect to the external provider), writing no state, so
        // the mutating-POST count is 30 not 32. The external-login *callback* GET is likewise treated
        // as non-mutating (session sign-in only); AccountController.Login POST is treated as mutating.
        // That session-vs-state nuance is the inventory's; the load-bearing pins are the explicit
        // named sets (7 AFT POSTs, 25 unprotected POSTs, 23 mutating GETs) in the fixtures.
        private static readonly HashSet<string> MutatingKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            // --- AccountController: 7 mutating POSTs ---
            "AccountController.Disassociate(String, String)",
            "AccountController.EditProfile(UserProfileFormModel)",
            "AccountController.ExternalLoginConfirmation(ExternalLoginConfirmationViewModel, String)",
            "AccountController.Login(LoginViewModel, String)",
            "AccountController.Manage(ManageUserViewModel)",
            "AccountController.Register(RegisterViewModel)",
            "AccountController.UploadImage(UploadImageViewModel)",
            // --- AccountController: 6 mutating GETs ---
            "AccountController.LinkLoginCallback()",
            "AccountController.LogOff()",
            "AccountController.FollowRequest(String)",
            "AccountController.AcceptRequest(String, String)",
            "AccountController.RejectRequest(String, String)",
            "AccountController.Unfollow(String)",

            // --- GoalController: 10 mutating POSTs ---
            "GoalController.Create(GoalFormModel)",
            "GoalController.Edit(GoalFormModel)",
            "GoalController.GoalStatus(Int32, Int32)",
            "GoalController.DeleteConfirmed(Int32)",
            "GoalController.SaveUpdate(UpdateFormModel)",
            "GoalController.InviteUser(Int32, String)",
            "GoalController.SaveComment(CommentFormModel)",
            "GoalController.InviteEmail(InviteEmailFormModel)",
            "GoalController.EditUpdate(UpdateFormModel)",
            "GoalController.DeleteConfirmedUpdate(Int32)",
            // --- GoalController: 5 mutating GETs ---
            "GoalController.SupportGoal(Int32)",
            "GoalController.SupportGoalNow(Int32)",
            "GoalController.UnSupportGoal(Int32)",
            "GoalController.SupportUpdate(Int32)",
            "GoalController.UnSupportUpdate(Int32)",

            // --- GroupController: 13 mutating POSTs ---
            "GroupController.CreateGroup(GroupFormModel)",
            "GroupController.CreateFocus(FocusFormModel)",
            "GroupController.DeleteConfirmedFocus(Int32)",
            "GroupController.EditFocus(FocusFormModel)",
            "GroupController.EditGroup(GroupFormModel)",
            "GroupController.DeleteConfirmedGroup(Int32)",
            "GroupController.CreateGoal(GroupGoalFormModel)",
            "GroupController.EditGoal(GroupGoalFormModel)",
            "GroupController.DeleteConfirmed(Int32)",
            "GroupController.SaveComment(GroupCommentFormModel)",
            "GroupController.InviteEmail(InviteEmailFormModel)",
            "GroupController.EditUpdate(GroupUpdateFormModel)",
            "GroupController.DeleteConfirmedUpdate(Int32)",
            // --- GroupController: 10 mutating GETs ---
            "GroupController.SaveUpdate(GroupUpdateFormModel)",
            "GroupController.InviteUser(Int32, String)",
            "GroupController.JoinGroup(Int32)",
            "GroupController.GroupJoinRequest(Int32)",
            "GroupController.AcceptRequest(Int32, String)",
            "GroupController.RejectRequest(Int32, String)",
            "GroupController.GoalStatus(Int32, Int32)",
            "GroupController.SupportUpdate(Int32)",
            "GroupController.UnSupportUpdate(Int32)",
            "GroupController.DeleteMember(String, Int32)",

            // --- EmailRequestController: 2 mutating GETs ---
            "EmailRequestController.AddGroupUser()",
            "EmailRequestController.AddSupportToGoal()",
        };

        // The 149 pinned rows. See file header for provenance. Ordered controller, then action,
        // then signature (matching the walker's ordering) so the diff reads top to bottom.
        private static readonly ControllerSurfaceRow[] BaseRows =
        {
            Row("AccountController", "AcceptRequest", "(String, String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Disassociate", "(String, String)", "POST", null, mA:false, mAnon:false, aft:true, child:false),
            Row("AccountController", "EditBasicInfo", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "EditPersonalInfo", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "EditProfile", "(UserProfileFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "ExternalLogin", "(String, String)", "POST", null, mA:false, mAnon:true, aft:true, child:false),
            Row("AccountController", "ExternalLoginCallback", "(String)", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "ExternalLoginConfirmation", "(ExternalLoginConfirmationViewModel, String)", "POST", null, mA:false, mAnon:true, aft:true, child:false),
            Row("AccountController", "ExternalLoginFailure", "()", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "FollowRequest", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Followers", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Followings", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "ImageUpload", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "LinkLogin", "(String)", "POST", null, mA:false, mAnon:false, aft:true, child:false),
            Row("AccountController", "LinkLoginCallback", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "LogOff", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Login", "(LoginViewModel, String)", "POST", null, mA:false, mAnon:true, aft:true, child:false),
            Row("AccountController", "Login", "(String)", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "LoginPartial", "()", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "Manage", "(ManageUserViewModel)", "POST", null, mA:false, mAnon:false, aft:true, child:false),
            Row("AccountController", "Manage", "(Nullable`1)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Register", "()", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "Register", "(RegisterViewModel)", "POST", null, mA:false, mAnon:true, aft:true, child:false),
            Row("AccountController", "RegisterPartial", "()", "", null, mA:false, mAnon:true, aft:false, child:false),
            Row("AccountController", "RejectRequest", "(String, String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "RemoveAccountList", "()", "", null, mA:false, mAnon:false, aft:false, child:true),
            Row("AccountController", "SearchUser", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "Unfollow", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "UploadImage", "(UploadImageViewModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("AccountController", "UserProfile", "(String)", "GET", null, mA:false, mAnon:false, aft:false, child:false),
            Row("EmailRequestController", "AddGroupUser", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("EmailRequestController", "AddSupportToGoal", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Create", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Create", "(GoalFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Delete", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DeleteConfirmed", "(Int32)", "POST", "Delete", mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DeleteConfirmedUpdate", "(Int32)", "POST", "DeleteUpdate", mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DeleteUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DisplayCommentCount", "(Int32)", "GET", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DisplayComments", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DisplayUpdateSupportCount", "(Int32)", "GET", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "DisplayUpdates", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Edit", "(GoalFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Edit", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "EditUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "EditUpdate", "(UpdateFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "FollowedGoal", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "GetGoalReport", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "GoalList", "(String, String, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "GoalProgress", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "GoalStatus", "(Int32, Int32)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "GoalsFollowing", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Goalslist", "(Int32, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Index", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "InviteEmail", "(InviteEmailFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "InviteUser", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "InviteUser", "(Int32, String)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "ListOfGoals", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "MyGoal", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "MyGoals", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "NoOfComments", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "NoOfSupports", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Reportpage", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SaveComment", "(CommentFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SaveUpdate", "(UpdateFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SearchGoal", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SearchUser", "(String, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SupportGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SupportGoalNow", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SupportInvitation", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SupportUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "Supporters", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "SupportersOfUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "UnSupportGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GoalController", "UnSupportUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "AcceptRequest", "(Int32, String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateFocus", "(FocusFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateFocus", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateGoal", "(GroupGoalFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateGroup", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "CreateGroup", "(GroupFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteConfirmed", "(Int32)", "POST", "DeleteGoal", mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteConfirmedFocus", "(Int32)", "POST", "DeleteFocus", mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteConfirmedGroup", "(Int32)", "POST", "DeleteGroup", mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteConfirmedUpdate", "(Int32)", "POST", "DeleteUpdate", mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteFocus", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteGroup", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteMember", "(String, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DeleteUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DisplayCommentCount", "(Int32)", "GET", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DisplayComments", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DisplayUpdateSupportCount", "(Int32)", "GET", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "DisplayUpdates", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditFocus", "(FocusFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditFocus", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditGoal", "(GroupGoalFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditGroup", "(GroupFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditGroup", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditUpdate", "(GroupUpdateFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "EditUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Focus", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "FollowedGroups", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Following", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GetGoalReport", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GetNumberOfRequests", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GoalProgress", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GoalStatus", "(Int32, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GroupGoal", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GroupJoinRequest", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GroupList", "(GroupFilter, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GroupViewOfUser", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "GroupsView", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Groupslist", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Index", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "InviteEmail", "(InviteEmailFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "InviteUser", "(Int32, String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "InviteUsers", "(Int32)", "", "InviteUsers", mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "JoinGroup", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "JoinedUsers", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "ListOfGroups", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Members", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "MyGroups", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "NoOfComments", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "NoOfSupports", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "NoOfUsers", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "RejectRequest", "(Int32, String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "Reportpage", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SaveComment", "(GroupCommentFormModel)", "POST", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SaveUpdate", "(GroupUpdateFormModel)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SearchMemberForGoalAssigning", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SearchUserForGroup", "(String, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "ShowAllRequests", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SupportUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "SupportersOfUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "UnSupportUpdate", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("GroupController", "UsersList", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "About", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "Contact", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "GetNotifications", "(Int32, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "GetNotifications", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "Index", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("HomeController", "UserNotification", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("NotificationController", "GetNotifications", "(Int32, Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("NotificationController", "GetNumberOfInvitations", "()", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("NotificationController", "Index", "(Int32)", "", null, mA:false, mAnon:false, aft:false, child:false),
            Row("SearchController", "SearchAll", "(String)", "", null, mA:false, mAnon:false, aft:false, child:false),
        };

        // Rows with the documentation-only Mutates flag stamped from MutatingKeys.
        public static IReadOnlyList<ControllerSurfaceRow> Rows()
        {
            foreach (var r in BaseRows)
                r.Mutates = MutatingKeys.Contains(r.Key);
            return BaseRows;
        }

        // Exposed so the fixture can assert existence/uniqueness of the documentation-only set:
        // every mutating key must resolve to exactly one row in the reflected/pinned surface.
        public static IReadOnlyCollection<string> MutatingActionKeys()
        {
            return MutatingKeys;
        }

        private static ControllerSurfaceRow Row(
            string controller, string action, string signature, string verbs, string actionName,
            bool mA, bool mAnon, bool aft, bool child)
        {
            return new ControllerSurfaceRow
            {
                Controller = controller,
                Action = action,
                Signature = signature,
                Verbs = verbs,
                ActionName = actionName,
                // Every controller carries a class-level [Authorize] except SearchController.
                ClassAuthorize = controller != "SearchController",
                ClassAllowAnonymous = false,
                MethodAuthorize = mA,
                MethodAllowAnonymous = mAnon,
                AntiForgery = aft,
                ChildActionOnly = child,
                Mutates = false
            };
        }
    }
}
