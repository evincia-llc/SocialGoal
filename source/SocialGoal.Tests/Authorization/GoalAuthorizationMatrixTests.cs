using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using SocialGoal.Data.Models;
using SocialGoal.Web.Controllers;
using SocialGoal.Web.ViewModels;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Behavioral authorization matrix -- GoalController (the exemplar). Each test
    /// INVOKES the real controller wired to real services/repositories over LocalDB
    /// and asserts the PERSISTED outcome via a fresh SocialGoalEntities.
    ///
    /// These are characterization pins of what the code DOES today, including the
    /// broken-object-level-authorization defect (LMRR R-007; secondary gap #1).
    /// PIN, NEVER FIX. Every BOLA-success test names the defect it pins and cross-
    /// refs the surface pin (EnforcementDefectPinTests: these actions sit in the 25
    /// unprotected POSTs / mutating GET set and carry no owner check). In Phase 2
    /// each BOLA-success pin becomes a negative (403) test under policy authorization.
    /// </summary>
    [TestFixture]
    public class GoalAuthorizationMatrixTests
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

        private GoalController NewGoalController(string actorId)
        {
            var asm = new ServiceAssembler();
            _assemblers.Add(asm);
            var controller = asm.BuildController<GoalController>();
            AuthenticatedActor.SignIn(controller, actorId);
            return controller;
        }

        // WHY: GoalController.Edit (POST, no [ValidateAntiForgeryToken], no owner check)
        // trusts editGoal.GoalId. An authenticated user who is NOT the owner can
        // overwrite any goal's fields. BOLA (R-007, gap #1). Surface cross-ref:
        // EnforcementDefectPinTests.The_other_twentyfive_POSTs... lists
        // "GoalController.Edit(GoalFormModel)" as unprotected.
        [Test]
        public void Edit_ByUnrelatedUser_MutatesOwnersGoal()
        {
            int goalId;
            using (var db = new SocialGoalEntities())
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-original").GoalId;

            var controller = NewGoalController(MatrixCast.UnrelatedId);
            var form = new GoalFormModel
            {
                GoalId = goalId,
                GoalName = "hijacked-by-unrelated",
                Desc = "changed by a non-owner",
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                UserId = MatrixCast.OwnerId
            };

            controller.Edit(form);

            using (var db = new SocialGoalEntities())
            {
                var goal = db.Goals.Find(goalId);
                Assert.That(goal, Is.Not.Null, "The owner's goal row must still exist.");
                Assert.That(goal.GoalName, Is.EqualTo("hijacked-by-unrelated"),
                    "An unrelated user's Edit persisted to the owner's goal (BOLA).");
            }
        }

        // WHY: GoalController.GoalStatus(id, goalid) trusts the caller-supplied
        // goalid and never checks ownership -- any authenticated user changes any
        // goal's status. State-changing action, BOLA (R-007). It is also one of the
        // unprotected POSTs. Covers the sibling GroupController.GoalStatus shape.
        [Test]
        public void GoalStatus_ByUnrelatedUser_MutatesOwnersGoalStatus()
        {
            int goalId;
            using (var db = new SocialGoalEntities())
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal").GoalId;

            var controller = NewGoalController(MatrixCast.UnrelatedId);

            var message = controller.GoalStatus(MatrixCast.StatusOnHoldId, goalId);

            Assert.That(message, Is.EqualTo("OnHold"));
            using (var db = new SocialGoalEntities())
            {
                var goal = db.Goals.Find(goalId);
                Assert.That(goal.GoalStatusId, Is.EqualTo(MatrixCast.StatusOnHoldId),
                    "An unrelated user changed the owner's goal status (BOLA).");
            }
        }

        // WHY: GoalController.DeleteConfirmed(id) (POST, ActionName "Delete") loads
        // by caller-supplied id and deletes with no owner check -- any authenticated
        // user deletes any goal. BOLA (R-007, gap #1); unprotected POST.
        [Test]
        public void DeleteConfirmed_ByUnrelatedUser_DeletesOwnersGoal()
        {
            int goalId;
            using (var db = new SocialGoalEntities())
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal").GoalId;

            var controller = NewGoalController(MatrixCast.UnrelatedId);

            controller.DeleteConfirmed(goalId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Goals.Find(goalId), Is.Null,
                    "An unrelated user deleted the owner's goal (BOLA).");
        }

        // WHY: GoalController.EditUpdate (POST) trusts newupdate.UpdateId and never
        // checks that the caller owns the parent goal -- any authenticated user
        // rewrites any update. BOLA (R-007); unprotected POST. Covers the sibling
        // GroupController.EditUpdate shape.
        [Test]
        public void EditUpdate_ByUnrelatedUser_MutatesUpdateOnOwnersGoal()
        {
            int updateId;
            using (var db = new SocialGoalEntities())
            {
                var goal = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal");
                updateId = MatrixCast.SeedUpdate(db, goal.GoalId, "owner-update").UpdateId;
            }

            var controller = NewGoalController(MatrixCast.UnrelatedId);
            var form = new UpdateFormModel { UpdateId = updateId, Updatemsg = "hijacked-update", status = 20, GoalId = GoalIdOf(updateId) };

            controller.EditUpdate(form);

            using (var db = new SocialGoalEntities())
            {
                var update = db.Updates.Find(updateId);
                Assert.That(update, Is.Not.Null);
                Assert.That(update.Updatemsg, Is.EqualTo("hijacked-update"),
                    "An unrelated user's EditUpdate persisted to another user's update (BOLA).");
            }
        }

        // WHY: GoalController.DeleteConfirmedUpdate(id) (POST, ActionName
        // "DeleteUpdate") deletes by caller-supplied id, no owner check. BOLA
        // (R-007); unprotected POST. Covers GroupController.DeleteConfirmedUpdate.
        [Test]
        public void DeleteConfirmedUpdate_ByUnrelatedUser_DeletesUpdateOnOwnersGoal()
        {
            int updateId;
            using (var db = new SocialGoalEntities())
            {
                var goal = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal");
                updateId = MatrixCast.SeedUpdate(db, goal.GoalId, "owner-update").UpdateId;
            }

            var controller = NewGoalController(MatrixCast.UnrelatedId);

            controller.DeleteConfirmedUpdate(updateId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Updates.Find(updateId), Is.Null,
                    "An unrelated user deleted another user's update (BOLA).");
        }

        // WHY (correctly-scoped counter-example): GoalController.UnSupportGoal(id)
        // derives the acting user from User.Identity.GetUserId() and deletes only
        // Support(GoalId, callerId). So an unrelated user cannot remove the OWNER's
        // support -- nothing matches, nothing is deleted. This is the safe binding
        // shape shared by Support*/UnSupport* on both goals and updates: scope comes
        // from the principal, not a caller-supplied owner id.
        [Test]
        public void UnSupportGoal_ByUnrelatedUser_DoesNotRemoveOwnersSupport()
        {
            int goalId;
            using (var db = new SocialGoalEntities())
            {
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal").GoalId;
                MatrixCast.SeedSupport(db, goalId, MatrixCast.OwnerId);
            }

            var controller = NewGoalController(MatrixCast.UnrelatedId);

            controller.UnSupportGoal(goalId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Support.Count(s => s.GoalId == goalId && s.UserId == MatrixCast.OwnerId),
                    Is.EqualTo(1), "The owner's support must survive an unrelated user's UnSupportGoal.");
        }

        // WHY (correctly-scoped positive): SupportGoal(id) records the supporter as
        // User.Identity.GetUserId() -- the supporter id cannot be forged. There is no
        // gate on supporting (any user may support any goal), but the identity is
        // caller-bound. This is the safe create-side shape shared by SupportGoal/
        // SupportGoalNow/SupportUpdate; UnSupportGoal above pins the delete side.
        [Test]
        public void SupportGoal_ByAnyUser_RecordsSupportAsCallerNotAForgedSupporter()
        {
            int goalId;
            using (var db = new SocialGoalEntities())
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "owner-goal").GoalId;

            var controller = NewGoalController(MatrixCast.UnrelatedId);

            controller.SupportGoal(goalId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Support.Count(s => s.GoalId == goalId && s.UserId == MatrixCast.UnrelatedId),
                    Is.EqualTo(1), "The support was recorded with the caller as supporter.");
        }

        private static int GoalIdOf(int updateId)
        {
            using (var db = new SocialGoalEntities())
                return db.Updates.Find(updateId).GoalId;
        }
    }
}
