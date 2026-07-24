using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SocialGoal.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastLoginTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Activated = table.Column<bool>(type: "bit", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentUsers",
                columns: table => new
                {
                    CommentUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentUsers", x => x.CommentUserId);
                });

            migrationBuilder.CreateTable(
                name: "FollowRequests",
                columns: table => new
                {
                    FollowRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequests", x => x.FollowRequestId);
                });

            migrationBuilder.CreateTable(
                name: "GoalStatus",
                columns: table => new
                {
                    GoalStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalStatusType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalStatus", x => x.GoalStatusId);
                });

            migrationBuilder.CreateTable(
                name: "GroupCommentUsers",
                columns: table => new
                {
                    GroupCommentUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupCommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupCommentUsers", x => x.GroupCommentUserId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "GroupUpdateUsers",
                columns: table => new
                {
                    GroupUpdateUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupUpdateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUpdateUsers", x => x.GroupUpdateUserId);
                });

            migrationBuilder.CreateTable(
                name: "GroupUsers",
                columns: table => new
                {
                    GroupUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Admin = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUsers", x => x.GroupUserId);
                });

            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    MetricId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.MetricId);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationTokens",
                columns: table => new
                {
                    RegistrationTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationTokens", x => x.RegistrationTokenId);
                });

            migrationBuilder.CreateTable(
                name: "SecurityTokens",
                columns: table => new
                {
                    SecurityTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityTokens", x => x.SecurityTokenId);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateEdited = table.Column<DateTime>(type: "datetime", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime", nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<double>(type: "float", nullable: true),
                    ContactNo = table.Column<double>(type: "float", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserProfileId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    User_Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "IdentityUserClaim_User",
                        column: x => x.User_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.UserId, x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "IdentityUserLogin_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "IdentityUserRole_Role",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "IdentityUserRole_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FollowUsers",
                columns: table => new
                {
                    FollowUserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FromUserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApplicationUser_Id = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    ApplicationUser_Id1 = table.Column<string>(type: "nvarchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUsers", x => x.FollowUserId);
                    table.ForeignKey(
                        name: "ApplicationUser_FollowFromUser",
                        column: x => x.ApplicationUser_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "ApplicationUser_FollowToUser",
                        column: x => x.ApplicationUser_Id1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FollowUser_FromUser",
                        column: x => x.FromUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FollowUser_ToUser",
                        column: x => x.ToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Foci",
                columns: table => new
                {
                    FocusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FocusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Foci", x => x.FocusId);
                    table.ForeignKey(
                        name: "Group_Foci",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupInvitations",
                columns: table => new
                {
                    GroupInvitationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupInvitations", x => x.GroupInvitationId);
                    table.ForeignKey(
                        name: "GroupInvitation_Group",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRequests",
                columns: table => new
                {
                    GroupRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Accepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRequests", x => x.GroupRequestId);
                    table.ForeignKey(
                        name: "ApplicationUser_GroupRequests",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "GroupRequest_Group",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalName = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Target = table.Column<double>(type: "float", nullable: true),
                    GoalType = table.Column<bool>(type: "bit", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    GoalStatusId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.ForeignKey(
                        name: "ApplicationUser_Goals",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "GoalStatus_Goals",
                        column: x => x.GoalStatusId,
                        principalTable: "GoalStatus",
                        principalColumn: "GoalStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Metric_Goals",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "MetricId");
                });

            migrationBuilder.CreateTable(
                name: "GroupGoals",
                columns: table => new
                {
                    GroupGoalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Target = table.Column<double>(type: "float", nullable: true),
                    MetricId = table.Column<int>(type: "int", nullable: true),
                    FocusId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    GoalStatusId = table.Column<int>(type: "int", nullable: false),
                    GroupUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedGroupUserId = table.Column<int>(type: "int", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupGoals", x => x.GroupGoalId);
                    table.ForeignKey(
                        name: "Focus_GroupGoals",
                        column: x => x.FocusId,
                        principalTable: "Foci",
                        principalColumn: "FocusId");
                    table.ForeignKey(
                        name: "GroupGoal_GoalStatus",
                        column: x => x.GoalStatusId,
                        principalTable: "GoalStatus",
                        principalColumn: "GoalStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "GroupGoal_Group",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "GroupUser_GroupGoals",
                        column: x => x.GroupUserId,
                        principalTable: "GroupUsers",
                        principalColumn: "GroupUserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Metric_GroupGoals",
                        column: x => x.MetricId,
                        principalTable: "Metrics",
                        principalColumn: "MetricId");
                });

            migrationBuilder.CreateTable(
                name: "SupportInvitations",
                columns: table => new
                {
                    SupportInvitationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportInvitations", x => x.SupportInvitationId);
                    table.ForeignKey(
                        name: "SupportInvitation_Goal",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supports",
                columns: table => new
                {
                    SupportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupportedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supports", x => x.SupportId);
                    table.ForeignKey(
                        name: "Support_Goal",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    UpdateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Updatemsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<double>(type: "float", nullable: true),
                    GoalId = table.Column<int>(type: "int", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.UpdateId);
                    table.ForeignKey(
                        name: "Goal_Updates",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupUpdates",
                columns: table => new
                {
                    GroupUpdateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Updatemsg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<double>(type: "float", nullable: true),
                    GroupGoalId = table.Column<int>(type: "int", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUpdates", x => x.GroupUpdateId);
                    table.ForeignKey(
                        name: "GroupUpdate_GroupGoal",
                        column: x => x.GroupGoalId,
                        principalTable: "GroupGoals",
                        principalColumn: "GroupGoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdateId = table.Column<int>(type: "int", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "Update_Comments",
                        column: x => x.UpdateId,
                        principalTable: "Updates",
                        principalColumn: "UpdateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UpdateSupports",
                columns: table => new
                {
                    UpdateSupportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UpdateId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateSupportedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateSupports", x => x.UpdateSupportId);
                    table.ForeignKey(
                        name: "UpdateSupport_Update",
                        column: x => x.UpdateId,
                        principalTable: "Updates",
                        principalColumn: "UpdateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupComments",
                columns: table => new
                {
                    GroupCommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupUpdateId = table.Column<int>(type: "int", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupComments", x => x.GroupCommentId);
                    table.ForeignKey(
                        name: "GroupComment_GroupUpdate",
                        column: x => x.GroupUpdateId,
                        principalTable: "GroupUpdates",
                        principalColumn: "GroupUpdateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupUpdateSupports",
                columns: table => new
                {
                    GroupUpdateSupportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupUpdateId = table.Column<int>(type: "int", nullable: false),
                    GroupUserId = table.Column<int>(type: "int", nullable: false),
                    UpdateSupportedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupUpdateSupports", x => x.GroupUpdateSupportId);
                    table.ForeignKey(
                        name: "GroupUpdateSupport_GroupUpdate",
                        column: x => x.GroupUpdateId,
                        principalTable: "GroupUpdates",
                        principalColumn: "GroupUpdateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GoalStatus",
                columns: new[] { "GoalStatusId", "GoalStatusType" },
                values: new object[,]
                {
                    { 1, "In Progress" },
                    { 2, "On Hold" },
                    { 3, "Completed" }
                });

            migrationBuilder.InsertData(
                table: "Metrics",
                columns: new[] { "MetricId", "Type" },
                values: new object[,]
                {
                    { 1, "%" },
                    { 2, "$" },
                    { 3, "$ M" },
                    { 4, "Rs" },
                    { 5, "Hours" },
                    { 6, "Km" },
                    { 7, "Kg" },
                    { 8, "Years" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_User_Id",
                table: "AspNetUserClaims",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UpdateId",
                table: "Comments",
                column: "UpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_Foci_GroupId",
                table: "Foci",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_ApplicationUser_Id",
                table: "FollowUsers",
                column: "ApplicationUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_ApplicationUser_Id1",
                table: "FollowUsers",
                column: "ApplicationUser_Id1");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_FromUserId",
                table: "FollowUsers",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_ToUserId",
                table: "FollowUsers",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_GoalStatusId",
                table: "Goals",
                column: "GoalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_MetricId",
                table: "Goals",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupComments_GroupUpdateId",
                table: "GroupComments",
                column: "GroupUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGoals_FocusId",
                table: "GroupGoals",
                column: "FocusId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGoals_GoalStatusId",
                table: "GroupGoals",
                column: "GoalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGoals_GroupId",
                table: "GroupGoals",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGoals_GroupUserId",
                table: "GroupGoals",
                column: "GroupUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupGoals_MetricId",
                table: "GroupGoals",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_GroupId",
                table: "GroupInvitations",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequests_GroupId",
                table: "GroupRequests",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequests_UserId",
                table: "GroupRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupUpdates_GroupGoalId",
                table: "GroupUpdates",
                column: "GroupGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupUpdateSupports_GroupUpdateId",
                table: "GroupUpdateSupports",
                column: "GroupUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportInvitations_GoalId",
                table: "SupportInvitations",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Supports_GoalId",
                table: "Supports",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_GoalId",
                table: "Updates",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_UpdateSupports_UpdateId",
                table: "UpdateSupports",
                column: "UpdateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CommentUsers");

            migrationBuilder.DropTable(
                name: "FollowRequests");

            migrationBuilder.DropTable(
                name: "FollowUsers");

            migrationBuilder.DropTable(
                name: "GroupComments");

            migrationBuilder.DropTable(
                name: "GroupCommentUsers");

            migrationBuilder.DropTable(
                name: "GroupInvitations");

            migrationBuilder.DropTable(
                name: "GroupRequests");

            migrationBuilder.DropTable(
                name: "GroupUpdateSupports");

            migrationBuilder.DropTable(
                name: "GroupUpdateUsers");

            migrationBuilder.DropTable(
                name: "RegistrationTokens");

            migrationBuilder.DropTable(
                name: "SecurityTokens");

            migrationBuilder.DropTable(
                name: "SupportInvitations");

            migrationBuilder.DropTable(
                name: "Supports");

            migrationBuilder.DropTable(
                name: "UpdateSupports");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GroupUpdates");

            migrationBuilder.DropTable(
                name: "Updates");

            migrationBuilder.DropTable(
                name: "GroupGoals");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "Foci");

            migrationBuilder.DropTable(
                name: "GroupUsers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GoalStatus");

            migrationBuilder.DropTable(
                name: "Metrics");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
