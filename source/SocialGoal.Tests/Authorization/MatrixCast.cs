using System;
using System.Data.Entity;
using System.Linq;
using SocialGoal.Data.Models;
using SocialGoal.Model.Models;
using SocialGoal.Tests.Data;

namespace SocialGoal.Tests.Authorization
{
    /// <summary>
    /// The standard cast and object seeders for the behavioral authorization
    /// matrix. Actor ids are STABLE strings so a test can build a claims
    /// principal and a form payload that reference the same user deterministically.
    ///
    /// Rows are seeded with a context SEPARATE from the one the controller wires
    /// through, so the controller reads them fresh from LocalDB and outcomes are
    /// asserted against persisted state (never against the controller's own
    /// context). Reuses the Data suite's table-name resolution / cleanup helpers.
    /// </summary>
    internal static class MatrixCast
    {
        // ---- Stable actor ids ----------------------------------------------
        internal const string OwnerId = "actor-owner";
        internal const string UnrelatedId = "actor-unrelated";
        internal const string GroupAdminId = "actor-group-admin";
        internal const string GroupMemberId = "actor-group-member";
        internal const string RequestSenderId = "actor-request-sender";
        internal const string RequestRecipientId = "actor-request-recipient";

        internal static readonly string[] AllActorIds =
        {
            OwnerId, UnrelatedId, GroupAdminId, GroupMemberId, RequestSenderId, RequestRecipientId
        };

        // Two GoalStatus rows with FORCED ids: id 1 is the Goal/GroupGoal ctor
        // default (so a form->entity map that omits GoalStatusId still satisfies
        // the FK); id 2 is the "switch to" target for the GoalStatus actions.
        internal const int StatusActiveId = 1;
        internal const int StatusOnHoldId = 2;

        // ---- Cleanup / standard fixture ------------------------------------

        /// <summary>
        /// FK-safe wipe of every table the matrix touches, children before parents.
        /// Called from each fixture's [SetUp] so tests are order-independent.
        /// </summary>
        internal static void CleanAll(SocialGoalEntities context)
        {
            Delete<UpdateSupport>(context);
            Delete<GroupUpdateSupport>(context);
            Delete<Comment>(context);
            Delete<GroupComment>(context);
            Delete<CommentUser>(context);
            Delete<GroupCommentUser>(context);
            Delete<GroupUpdateUser>(context);
            Delete<Update>(context);
            Delete<GroupUpdate>(context);
            Delete<Support>(context);
            Delete<SupportInvitation>(context);
            Delete<GroupGoal>(context);
            Delete<GroupUser>(context);
            Delete<GroupRequest>(context);
            Delete<GroupInvitation>(context);
            Delete<Focus>(context);
            Delete<FollowUser>(context);
            Delete<FollowRequest>(context);
            Delete<Goal>(context);
            Delete<Group>(context);
            Delete<GoalStatus>(context);
            Delete<Metric>(context);
            Delete<SecurityToken>(context);
            Delete<UserProfile>(context);
            Delete<ApplicationUser>(context);
        }

        private static void Delete<T>(SocialGoalEntities context) where T : class
        {
            context.Database.ExecuteSqlCommand("DELETE FROM " + TestDataHelper.QualifiedTable<T>(context));
        }

        /// <summary>Seeds the six cast users (each with a UserProfile) and the two GoalStatus rows.</summary>
        internal static void SeedStandardCast(SocialGoalEntities context)
        {
            foreach (var id in AllActorIds)
            {
                SeedUser(context, id);
                SeedUserProfile(context, id);
            }
            SeedGoalStatuses(context);
        }

        // ---- Individual seeders --------------------------------------------

        internal static ApplicationUser SeedUser(SocialGoalEntities context, string id)
        {
            var user = new ApplicationUser
            {
                Id = id,
                UserName = id,
                Email = id + "@example.test",
                FirstName = "First-" + id,
                LastName = "Last-" + id
            };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        internal static UserProfile SeedUserProfile(SocialGoalEntities context, string userId)
        {
            var profile = new UserProfile
            {
                UserId = userId,
                FirstName = "First-" + userId,
                LastName = "Last-" + userId,
                Email = userId + "@example.test"
            };
            context.UserProfile.Add(profile);
            context.SaveChanges();
            return profile;
        }

        /// <summary>Forces GoalStatus ids 1 and 2 via IDENTITY_INSERT so FK targets are deterministic.</summary>
        internal static void SeedGoalStatuses(SocialGoalEntities context)
        {
            var table = TestDataHelper.QualifiedTable<GoalStatus>(context); // [dbo].[GoalStatus]
            context.Database.ExecuteSqlCommand(
                "SET IDENTITY_INSERT " + table + " ON; " +
                "INSERT INTO " + table + " (GoalStatusId, GoalStatusType) VALUES (1, 'Active'), (2, 'OnHold'); " +
                "SET IDENTITY_INSERT " + table + " OFF;");
        }

        internal static Metric SeedMetric(SocialGoalEntities context, string type)
        {
            var metric = new Metric { Type = type };
            context.Metrics.Add(metric);
            context.SaveChanges();
            return metric;
        }

        internal static Goal SeedGoal(SocialGoalEntities context, string ownerId, string name)
        {
            var goal = new Goal
            {
                GoalName = name,
                Desc = "seeded",
                GoalType = false,
                GoalStatusId = StatusActiveId,
                UserId = ownerId,
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                CreatedDate = new DateTime(2020, 1, 1)
            };
            context.Goals.Add(goal);
            context.SaveChanges();
            return goal;
        }

        internal static Update SeedUpdate(SocialGoalEntities context, int goalId, string msg)
        {
            var update = new Update
            {
                GoalId = goalId,
                Updatemsg = msg,
                status = 10,
                UpdateDate = new DateTime(2020, 6, 1)
            };
            context.Updates.Add(update);
            context.SaveChanges();
            return update;
        }

        internal static Support SeedSupport(SocialGoalEntities context, int goalId, string userId)
        {
            var support = new Support
            {
                GoalId = goalId,
                UserId = userId,
                SupportedDate = new DateTime(2020, 6, 1)
            };
            context.Support.Add(support);
            context.SaveChanges();
            return support;
        }

        /// <summary>
        /// A group with an admin GroupUser (Admin=true) and a plain member
        /// (Admin=false). Returns the group; the caller reads GroupUser ids back
        /// through <see cref="GetGroupUserId"/> when needed.
        /// </summary>
        internal static Group SeedGroupWithMembers(SocialGoalEntities context, string groupName,
            string adminId, string memberId)
        {
            var group = new Group { GroupName = groupName, Description = "seeded", CreatedDate = new DateTime(2020, 1, 1) };
            context.Groups.Add(group);
            context.SaveChanges();

            context.GroupUsers.Add(new GroupUser { GroupId = group.GroupId, UserId = adminId, Admin = true, AddedDate = new DateTime(2020, 1, 1) });
            context.GroupUsers.Add(new GroupUser { GroupId = group.GroupId, UserId = memberId, Admin = false, AddedDate = new DateTime(2020, 1, 1) });
            context.SaveChanges();
            return group;
        }

        internal static int GetGroupUserId(SocialGoalEntities context, int groupId, string userId)
        {
            var gu = context.GroupUsers.Single(g => g.GroupId == groupId && g.UserId == userId);
            return gu.GroupUserId;
        }

        internal static GroupGoal SeedGroupGoal(SocialGoalEntities context, Group group,
            int ownerGroupUserId, string name)
        {
            var goal = new GroupGoal
            {
                GoalName = name,
                Description = "seeded",
                GroupId = group.GroupId,
                GroupUserId = ownerGroupUserId,
                GoalStatusId = StatusActiveId,
                StartDate = new DateTime(2020, 1, 1),
                EndDate = new DateTime(2020, 12, 31),
                CreatedDate = new DateTime(2020, 1, 1)
            };
            context.GroupGoal.Add(goal);
            context.SaveChanges();
            return goal;
        }

        internal static Focus SeedFocus(SocialGoalEntities context, int groupId, string name)
        {
            var focus = new Focus
            {
                FocusName = name,
                Description = "seeded",
                GroupId = groupId,
                CreatedDate = new DateTime(2020, 1, 1)
            };
            context.Focuses.Add(focus);
            context.SaveChanges();
            return focus;
        }

        internal static GroupRequest SeedGroupRequest(SocialGoalEntities context, int groupId, string userId)
        {
            var request = new GroupRequest { GroupId = groupId, UserId = userId, Accepted = false };
            context.GroupRequests.Add(request);
            context.SaveChanges();
            return request;
        }

        internal static FollowRequest SeedFollowRequest(SocialGoalEntities context, string fromUserId, string toUserId)
        {
            var request = new FollowRequest { FromUserId = fromUserId, ToUserId = toUserId, Accepted = false };
            context.FollowRequest.Add(request);
            context.SaveChanges();
            return request;
        }

        internal static FollowUser SeedFollowUser(SocialGoalEntities context, string fromUserId, string toUserId)
        {
            var follow = new FollowUser
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Accepted = true,
                AddedDate = new DateTime(2020, 1, 1)
            };
            context.FollowUser.Add(follow);
            context.SaveChanges();
            return follow;
        }

        internal static Guid SeedSecurityToken(SocialGoalEntities context, int actualId)
        {
            var token = Guid.NewGuid();
            context.SecurityTokens.Add(new SecurityToken { Token = token, ActualID = actualId });
            context.SaveChanges();
            return token;
        }
    }
}
