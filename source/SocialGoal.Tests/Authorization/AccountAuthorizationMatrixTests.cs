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
    /// Behavioral authorization matrix -- AccountController. Same discipline: invoke
    /// the real controller (with a real UserManager over LocalDB satisfying its ctor)
    /// and assert persisted state via a fresh context. PIN, NEVER FIX.
    ///
    /// Headline defects (inventory §BOLA-critical): EditProfile trusts
    /// editedProfile.UserId, so any authenticated user edits ANY user's profile;
    /// Accept/Reject follow requests are GET-reachable and take both party ids as
    /// caller-supplied params (no recipient check), so a non-party forges/destroys
    /// follow relationships. Unfollow is the correctly-scoped counter-example.
    /// LMRR R-007; secondary gap #1 (BOLA) and #2 (state-changing GET).
    /// </summary>
    [TestFixture]
    public class AccountAuthorizationMatrixTests
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

        private AccountController NewAccountController(string actorId)
        {
            var asm = new ServiceAssembler();
            _assemblers.Add(asm);
            var controller = asm.BuildController<AccountController>();
            AuthenticatedActor.SignIn(controller, actorId);
            return controller;
        }

        // WHY: EditProfile (POST) rebinds the target user from editedProfile.UserId
        // and writes FirstName/LastName/Email to WHOEVER that id names -- no check
        // that the caller owns the profile. Any authenticated user edits any user's
        // profile. BOLA (R-007, gap #1); unprotected POST.
        [Test]
        public void EditProfile_ByUnrelatedUser_EditsOwnersProfile()
        {
            int ownerProfileId;
            using (var db = new SocialGoalEntities())
                ownerProfileId = db.UserProfile.Single(p => p.UserId == MatrixCast.OwnerId).UserProfileId;

            var controller = NewAccountController(MatrixCast.UnrelatedId);
            var form = new UserProfileFormModel
            {
                UserProfileId = ownerProfileId,
                UserId = MatrixCast.OwnerId,
                FirstName = "Hijacked",
                LastName = "ByUnrelated",
                Email = "owner@example.test"
            };

            controller.EditProfile(form);

            using (var db = new SocialGoalEntities())
                Assert.That(db.Users.Single(u => u.Id == MatrixCast.OwnerId).FirstName, Is.EqualTo("Hijacked"),
                    "An unrelated user edited the owner's profile (BOLA).");
        }

        // WHY: AccountController.AcceptRequest(touserid, fromuserid) is a GET-reachable
        // mutation. Both party ids are caller-supplied and there is NO recipient
        // check, so a third party forges a follow relationship between two other
        // users. BOLA (R-007) + state-changing GET (gap #2).
        [Test]
        public void AcceptRequest_ByNonRecipient_ForgesFollowRelationship()
        {
            using (var db = new SocialGoalEntities())
                MatrixCast.SeedFollowRequest(db, MatrixCast.RequestSenderId, MatrixCast.RequestRecipientId);

            var controller = NewAccountController(MatrixCast.UnrelatedId);

            // Signature: AcceptRequest(touserid, fromuserid).
            controller.AcceptRequest(MatrixCast.RequestRecipientId, MatrixCast.RequestSenderId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.FollowUser.Count(f => f.FromUserId == MatrixCast.RequestSenderId
                                                   && f.ToUserId == MatrixCast.RequestRecipientId),
                    Is.EqualTo(1), "A non-recipient forged an accepted follow relationship (BOLA).");
        }

        // WHY: AccountController.RejectRequest(toUserId, fromuserId) is a GET-reachable
        // mutation with no recipient check -- a third party destroys someone else's
        // pending follow request. BOLA (R-007) + state-changing GET (gap #2).
        [Test]
        public void RejectRequest_ByNonRecipient_DeletesPendingRequest()
        {
            using (var db = new SocialGoalEntities())
                MatrixCast.SeedFollowRequest(db, MatrixCast.RequestSenderId, MatrixCast.RequestRecipientId);

            var controller = NewAccountController(MatrixCast.UnrelatedId);

            // Signature: RejectRequest(toUserId, fromuserId).
            controller.RejectRequest(MatrixCast.RequestRecipientId, MatrixCast.RequestSenderId);

            using (var db = new SocialGoalEntities())
                Assert.That(db.FollowRequest.Count(r => r.FromUserId == MatrixCast.RequestSenderId
                                                      && r.ToUserId == MatrixCast.RequestRecipientId),
                    Is.EqualTo(0), "A non-recipient deleted someone else's follow request (BOLA).");
        }

        // WHY (correctly-scoped positive): FollowRequest(id) derives the sender from
        // User.Identity.GetUserId() -- the FromUserId cannot be forged. There is no
        // gate on SENDING a request (any user may request to follow anyone), but the
        // sender identity is caller-bound. Contrast with Accept/Reject above whose
        // party ids ARE caller-supplied. Safe binding shape (inventory §Actor-binding).
        [Test]
        public void FollowRequest_ByAnyUser_RecordsRequestFromCallerNotAForgedSender()
        {
            var controller = NewAccountController(MatrixCast.UnrelatedId);

            controller.FollowRequest(MatrixCast.RequestRecipientId);

            using (var db = new SocialGoalEntities())
            {
                Assert.That(db.FollowRequest.Count(r => r.FromUserId == MatrixCast.UnrelatedId
                                                      && r.ToUserId == MatrixCast.RequestRecipientId),
                    Is.EqualTo(1), "The request was recorded with the caller as sender.");
            }
        }

        // WHY (correctly-scoped counter-example): Unfollow(id) derives the acting
        // user from User.Identity.GetUserId() and looks up FollowUser(From=caller,
        // To=id). An unrelated caller matches no such row, so it cannot delete the
        // OWNER's follow. The lookup returns null and DbSet.Remove(null) throws --
        // an INCIDENTAL gate (the accidental effect of caller-bound scope, NOT an
        // authorization check). Exceptions are outcomes: pinned as-is, not "fixed".
        [Test]
        public void Unfollow_ByUnrelatedUser_DoesNotDeleteOwnersFollow()
        {
            using (var db = new SocialGoalEntities())
                MatrixCast.SeedFollowUser(db, MatrixCast.OwnerId, MatrixCast.RequestRecipientId);

            var controller = NewAccountController(MatrixCast.UnrelatedId);

            Assert.Throws<ArgumentNullException>(() => controller.Unfollow(MatrixCast.RequestRecipientId),
                "Unfollow finds no row for the unrelated caller and DbSet.Remove(null) throws (incidental gate).");

            using (var db = new SocialGoalEntities())
                Assert.That(db.FollowUser.Count(f => f.FromUserId == MatrixCast.OwnerId
                                                   && f.ToUserId == MatrixCast.RequestRecipientId),
                    Is.EqualTo(1), "The owner's follow survives an unrelated user's Unfollow.");
        }
    }
}
