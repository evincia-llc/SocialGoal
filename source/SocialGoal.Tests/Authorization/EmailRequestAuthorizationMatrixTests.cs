using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SocialGoal.Data.Models;
using SocialGoal.Web.Controllers;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// Behavioral authorization matrix -- EmailRequestController. These two actions
    /// (AddGroupUser, AddSupportToGoal) are the token-mediated GET flows: they read
    /// a token id from TempData, resolve it to a group/goal via SecurityToken, and
    /// join/support AS THE CURRENT USER. The authorization observation: any
    /// authenticated user who obtains (or guesses) a token joins/supports as
    /// themselves -- there is no binding of the token to the invited principal.
    /// State-changing GET (gap #2) with a shared-secret token as the only control.
    ///
    /// Session-facade caveat (per harness design): both actions end with
    /// SocialGoalSessionFacade.Remove(...), which dereferences HttpContext.Current
    /// .Session. In-process HttpContext.Current is null, so that final line throws
    /// NullReferenceException -- but the join/support and the token deletion have
    /// ALREADY committed by then. We therefore pin BOTH facts: the mutation persists,
    /// and the action throws at the session facade. This avoids fabricating a fake
    /// HttpSessionState and is an honest characterization of in-process behavior.
    /// PIN, NEVER FIX.
    /// </summary>
    [TestFixture]
    public class EmailRequestAuthorizationMatrixTests
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

        private EmailRequestController NewEmailRequestController(string actorId)
        {
            var asm = new ServiceAssembler();
            _assemblers.Add(asm);
            var controller = asm.BuildController<EmailRequestController>();
            AuthenticatedActor.SignIn(controller, actorId);
            return controller;
        }

        // WHY: AddGroupUser resolves TempData["grToken"] to a group and adds the
        // CURRENT user as a member. Any authenticated user holding the token joins as
        // themselves; the token is not bound to the invited address. The membership
        // and token deletion commit before the incidental session-facade NRE.
        [Test]
        public void AddGroupUser_WithToken_JoinsCallerToGroup_ThenThrowsAtSessionFacade()
        {
            int groupId;
            Guid token;
            using (var db = new SocialGoalEntities())
            {
                groupId = MatrixCast.SeedGroupWithMembers(db, "invited-group",
                    MatrixCast.GroupAdminId, MatrixCast.GroupMemberId).GroupId;
                token = MatrixCast.SeedSecurityToken(db, groupId);
            }

            var controller = NewEmailRequestController(MatrixCast.UnrelatedId);
            controller.TempData["grToken"] = token;

            Assert.Throws<NullReferenceException>(() => controller.AddGroupUser(),
                "The join commits, then SocialGoalSessionFacade.Remove dereferences a null HttpContext.Current.Session.");

            using (var db = new SocialGoalEntities())
            {
                Assert.That(db.GroupUsers.Count(gu => gu.GroupId == groupId && gu.UserId == MatrixCast.UnrelatedId),
                    Is.EqualTo(1), "The token holder joined the group as themselves (persisted before the NRE).");
                Assert.That(db.SecurityTokens.Count(s => s.Token == token), Is.EqualTo(0),
                    "The token was consumed before the NRE.");
            }
        }

        // WHY: AddSupportToGoal resolves TempData["goToken"] to a goal and adds the
        // CURRENT user as a supporter. Same token-holder-joins-as-themselves shape;
        // support and token deletion commit before the incidental session-facade NRE.
        [Test]
        public void AddSupportToGoal_WithToken_SupportsAsCaller_ThenThrowsAtSessionFacade()
        {
            int goalId;
            Guid token;
            using (var db = new SocialGoalEntities())
            {
                goalId = MatrixCast.SeedGoal(db, MatrixCast.OwnerId, "invited-goal").GoalId;
                token = MatrixCast.SeedSecurityToken(db, goalId);
            }

            var controller = NewEmailRequestController(MatrixCast.UnrelatedId);
            controller.TempData["goToken"] = token;

            Assert.Throws<NullReferenceException>(() => controller.AddSupportToGoal(),
                "The support commits, then SocialGoalSessionFacade.Remove dereferences a null HttpContext.Current.Session.");

            using (var db = new SocialGoalEntities())
            {
                Assert.That(db.Support.Count(s => s.GoalId == goalId && s.UserId == MatrixCast.UnrelatedId),
                    Is.EqualTo(1), "The token holder supported the goal as themselves (persisted before the NRE).");
                Assert.That(db.SecurityTokens.Count(s => s.Token == token), Is.EqualTo(0),
                    "The token was consumed before the NRE.");
            }
        }
    }
}
