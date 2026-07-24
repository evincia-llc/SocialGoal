using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SocialGoal.Data.Models;
using SocialGoal.Web.Controllers;
using SocialGoal.Web.ViewModels;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Behavioral authorization matrix -- GroupController. Same discipline as the
    /// Goal fixture: invoke the real controller over LocalDB and assert persisted
    /// state via a fresh context. PIN, NEVER FIX.
    ///
    /// The headline defect here (inventory §"Actor-binding patterns"): the
    /// GroupUser.Admin flag is WRITTEN but never READ as an authorization gate, and
    /// group membership is never asserted for group mutations. So admin, non-admin
    /// member, and unrelated user collapse to "any authenticated user" on every
    /// group mutation; and Accept/Reject accept both party ids as caller-supplied
    /// params (no recipient/admin check). BOLA (LMRR R-007; secondary gap #1).
    /// Cross-ref surface pins: EnforcementDefectPinTests (these actions are in the
    /// unprotected-POST set and the 23 GET-reachable mutations).
    /// </summary>
    [TestFixture]
    public class GroupAuthorizationMatrixTests
    {
        private readonly List<ServiceAssembler> _assemblers = new List<ServiceAssembler>();

        [SetUp]
        public void CleanAndSeedCast()
        {
            using (var context = new SocialGoalEntities())
            {
                MatrixCast.CleanAll(context);
                MatrixCast.SeedStandardCast(context);
            }
        }

        [TearDown]
        public void DisposeAssemblers()
        {
            foreach (var asm in _assemblers)
                asm.Dispose();
            _assemblers.Clear();
        }

        private GroupController NewGroupController(string actorId)
        {
            var asm = new ServiceAssembler();
            _assemblers.Add(asm);
            var controller = asm.BuildController<GroupController>();
            AuthenticatedActor.SignIn(controller, actorId);
            return controller;
        }

        // WHY: EditGroup (POST) never checks admin OR membership. A non-admin member
        // AND a completely unrelated user both mutate the group identically to the
        // admin -- proving the Admin flag never gates (inventory §3). BOLA (R-007).
        [Test]
        public void EditGroup_ByNonAdminMemberAndUnrelatedUser_BothMutateGroup()
        {
            foreach (var actorId in new[] { MatrixCast.GroupMemberId, MatrixCast.UnrelatedId })
            {
                int groupId;
                using (var db = new SocialGoalEntities())
                    groupId = MatrixCast.SeedGroupWithMembers(db, "group-for-" + actorId,
                        MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

                var controller = NewGroupController(actorId);
                var form = new GroupFormModel
                {
                    GroupId = groupId,
                    GroupName = "renamed-by-" + actorId,
                    Description = "changed by a non-admin"
                };

                controller.EditGroup(form);

                using (var db = new SocialGoalEntities())
                    Assert.That(db.Groups.Find(groupId).GroupName, Is.EqualTo("renamed-by-" + actorId),
                        actorId + " (not the group admin) mutated the group (BOLA -- Admin flag never gates).");
            }
        }

        // WHY: DeleteConfirmedGroup (POST) deletes by caller-supplied id, no admin
        // check -- any authenticated user deletes any group (and its memberships).
        [Test]
        public void DeleteConfirmedGroup_ByUnrelatedUser_DeletesGroup()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
                groupId = MatrixCast.SeedGroupWithMembers(db, "doomed-group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.DeleteConfirmedGroup(groupId);

            using (var db = new SocialGoalEntities())
            {
                Assert.That(db.Groups.Find(groupId), Is.Null, "An unrelated user deleted the group (BOLA).");
                Assert.That(db.GroupUsers.Count(gu => gu.GroupId == groupId), Is.EqualTo(0),
                    "Memberships were removed along with the group.");
            }
        }

        // WHY: DeleteMember(userId, groupId) is a GET-reachable mutation that removes
        // any member from any group with no admin check -- any authenticated user
        // evicts any member. BOLA (R-007) + state-changing GET (gap #2).
        [Test]
        public void DeleteMember_ByUnrelatedUser_RemovesMember()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.DeleteMember(MatrixCast.GroupMemberId, groupId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupUsers.Count(gu => gu.GroupId == groupId && gu.UserId == MatrixCast.GroupMemberId),
                    Is.EqualTo(0), "An unrelated user evicted a group member (BOLA).");
        }

        // WHY: EditGoal (POST) trusts editGoal.GroupGoalId and never checks membership
        // or admin -- any authenticated user rewrites any group goal. BOLA (R-007).
        [Test]
        public void EditGoal_ByUnrelatedUser_MutatesGroupGoal()
        {
            int groupGoalId, ownerGroupUserId, groupId;
            using (var db = new SocialGoalEntities())
            {
                var group = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId);
                groupId = group.GroupId;
                ownerGroupUserId = MatrixCast.GetGroupUserId(db, groupId, MatrixCast.GroupAdminId);
                groupGoalId = MatrixCast.SeedGroupGoal(db, group, ownerGroupUserId, "group-goal-original").GroupGoalId;
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);
            var form = new GroupGoalFormModel
            {
                GroupGoalId = groupGoalId,
                GoalName = "hijacked-group-goal",
                Description = "changed by a non-member",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                GroupUserId = ownerGroupUserId,
                GroupId = groupId
            };

            controller.EditGoal(form);

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupGoal.Find(groupGoalId).GoalName, Is.EqualTo("hijacked-group-goal"),
                    "An unrelated user mutated a group goal (BOLA).");
        }

        // WHY: DeleteConfirmed (POST, ActionName "DeleteGoal") deletes a group goal
        // by caller-supplied id with no membership/admin check. BOLA (R-007).
        [Test]
        public void DeleteConfirmed_ByUnrelatedUser_DeletesGroupGoal()
        {
            int groupGoalId;
            using (var db = new SocialGoalEntities())
            {
                var group = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId);
                var ownerGroupUserId = MatrixCast.GetGroupUserId(db, group.GroupId, MatrixCast.GroupAdminId);
                groupGoalId = MatrixCast.SeedGroupGoal(db, group, ownerGroupUserId, "group-goal").GroupGoalId;
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.DeleteConfirmed(groupGoalId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupGoal.Find(groupGoalId), Is.Null,
                    "An unrelated user deleted a group goal (BOLA).");
        }

        // WHY: GroupController.GoalStatus(id, goalid) is a GET-reachable mutation that
        // changes any group goal's status with no membership/admin check. BOLA
        // (R-007) + state-changing GET (gap #2). Sibling of GoalController.GoalStatus.
        [Test]
        public void GoalStatus_ByUnrelatedUser_MutatesGroupGoalStatus()
        {
            int groupGoalId;
            using (var db = new SocialGoalEntities())
            {
                var group = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId);
                var ownerGroupUserId = MatrixCast.GetGroupUserId(db, group.GroupId, MatrixCast.GroupAdminId);
                groupGoalId = MatrixCast.SeedGroupGoal(db, group, ownerGroupUserId, "group-goal").GroupGoalId;
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            var message = controller.GoalStatus(MatrixCast.StatusOnHoldId, groupGoalId);

            Assert.That(message, Is.EqualTo("OnHold"));
            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupGoal.Find(groupGoalId).GoalStatusId, Is.EqualTo(MatrixCast.StatusOnHoldId),
                    "An unrelated user changed a group goal's status (BOLA).");
        }

        // WHY: AcceptRequest(groupId, userId) is a GET-reachable mutation. Both the
        // group and the joining user are caller-supplied; there is no admin OR
        // recipient check, so ANY authenticated user can approve ANYONE into ANY
        // group. BOLA (R-007) + state-changing GET (gap #2).
        [Test]
        public void AcceptRequest_ByNonAdminNonMember_AddsRequesterToGroup()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
            {
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;
                MatrixCast.SeedGroupRequest(db, groupId, MatrixCast.RequestSenderId);
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.AcceptRequest(groupId, MatrixCast.RequestSenderId);

            using (var db = new SocialGoalEntities())
            {
                Assert.That(db.GroupUsers.Count(gu => gu.GroupId == groupId && gu.UserId == MatrixCast.RequestSenderId),
                    Is.EqualTo(1), "A non-admin, non-member approved a join request (BOLA).");
                Assert.That(db.GroupRequests.Count(r => r.GroupId == groupId && r.UserId == MatrixCast.RequestSenderId),
                    Is.EqualTo(0), "The approved request was consumed.");
            }
        }

        // WHY: RejectRequest(groupId, userId) is a GET-reachable mutation with no
        // admin check -- any authenticated user rejects anyone's join request.
        // BOLA (R-007) + state-changing GET (gap #2).
        [Test]
        public void RejectRequest_ByNonAdminNonMember_DeletesRequest()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
            {
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;
                MatrixCast.SeedGroupRequest(db, groupId, MatrixCast.RequestSenderId);
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.RejectRequest(groupId, MatrixCast.RequestSenderId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupRequests.Count(r => r.GroupId == groupId && r.UserId == MatrixCast.RequestSenderId),
                    Is.EqualTo(0), "A non-admin rejected someone else's join request (BOLA).");
        }

        // WHY: JoinGroup(id) is a GET-reachable mutation. It is self-scoped (the new
        // member is User.Identity.GetUserId(), not spoofable) BUT there is no gate on
        // joining: no invitation and no approval are required -- any authenticated
        // user adds themselves to any group. State-changing GET (gap #2); open-join
        // authorization hole distinct from the caller-supplied-id BOLA above.
        [Test]
        public void JoinGroup_ByAnyUser_JoinsWithoutInvitationOrApproval()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.JoinGroup(groupId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupUsers.Count(gu => gu.GroupId == groupId && gu.UserId == MatrixCast.UnrelatedId),
                    Is.EqualTo(1), "Any authenticated user self-joined the group with no invitation/approval.");
        }

        // WHY: CreateFocus (POST) writes a focus into a caller-supplied group with no
        // membership/admin check -- any authenticated user adds a focus to any group.
        // BOLA (R-007); unprotected POST.
        [Test]
        public void CreateFocus_ByUnrelatedUser_CreatesFocusInGroup()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

            var controller = NewGroupController(MatrixCast.UnrelatedId);
            var form = new FocusFormModel { FocusName = "focus-by-unrelated", Description = "added by a non-member", GroupId = groupId };

            controller.CreateFocus(form);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Focuses.Count(f => f.GroupId == groupId && f.FocusName == "focus-by-unrelated"),
                    Is.EqualTo(1), "An unrelated user created a focus in a group they do not belong to (BOLA).");
        }

        // WHY: EditFocus (POST) trusts the caller-supplied FocusId/GroupId with no
        // membership/admin check -- any authenticated user rewrites any focus.
        // BOLA (R-007); unprotected POST.
        [Test]
        public void EditFocus_ByUnrelatedUser_MutatesFocus()
        {
            int focusId, groupId;
            using (var db = new SocialGoalEntities())
            {
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;
                focusId = MatrixCast.SeedFocus(db, groupId, "focus-original").FocusId;
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);
            var form = new FocusFormModel { FocusId = focusId, FocusName = "hijacked-focus", Description = "changed", GroupId = groupId };

            controller.EditFocus(form);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Focuses.Find(focusId).FocusName, Is.EqualTo("hijacked-focus"),
                    "An unrelated user mutated a focus (BOLA).");
        }

        // WHY: DeleteConfirmedFocus (POST, ActionName "DeleteFocus") deletes by
        // caller-supplied id with no membership/admin check. BOLA (R-007).
        [Test]
        public void DeleteConfirmedFocus_ByUnrelatedUser_DeletesFocus()
        {
            int focusId;
            using (var db = new SocialGoalEntities())
            {
                var groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;
                focusId = MatrixCast.SeedFocus(db, groupId, "focus").FocusId;
            }

            var controller = NewGroupController(MatrixCast.UnrelatedId);

            controller.DeleteConfirmedFocus(focusId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Focuses.Find(focusId), Is.Null, "An unrelated user deleted a focus (BOLA).");
        }

        // WHY (incidental gate, NOT authorization): CreateGoal (POST) looks up the
        // caller's GroupUser via GetGroupUser(caller, groupId) and dereferences it
        // for GroupUserId. For a NON-member that lookup is null, so the action throws
        // NullReferenceException before persisting. This accidental effect (there is
        // no membership CHECK -- the code simply assumes the caller is a member) must
        // not be mistaken for an access control. Pinned as-is; not "fixed".
        [Test]
        public void CreateGoal_ByNonMember_ThrowsNullReference_NoGoalPersisted()
        {
            int groupId;
            using (var db = new SocialGoalEntities())
                groupId = MatrixCast.SeedGroupWithMembers(db, "group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;

            var controller = NewGroupController(MatrixCast.UnrelatedId);
            var form = new GroupGoalFormModel
            {
                GoalName = "non-member-goal",
                Description = "should not persist",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                GroupId = groupId
            };

            Assert.Throws<NullReferenceException>(() => controller.CreateGoal(form),
                "A non-member's GroupUser lookup is null and is dereferenced (incidental gate, not an authz check).");

            using (var db = new SocialGoalEntities())
                Assert.That(db.GroupGoal.Count(g => g.GroupId == groupId), Is.EqualTo(0),
                    "No group goal was persisted.");
        }
    }
}
