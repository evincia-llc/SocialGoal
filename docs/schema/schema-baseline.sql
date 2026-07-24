create table [dbo].[Comments] (
    [CommentId] [int] not null identity,
    [CommentText] [nvarchar](250) null,
    [UpdateId] [int] not null,
    [CommentDate] [datetime] not null,
    primary key ([CommentId])
);
create table [dbo].[CommentUsers] (
    [CommentUserId] [int] not null identity,
    [CommentId] [int] not null,
    [UserId] [nvarchar](max) not null,
    primary key ([CommentUserId])
);
create table [dbo].[Foci] (
    [FocusId] [int] not null identity,
    [FocusName] [nvarchar](50) null,
    [Description] [nvarchar](100) null,
    [GroupId] [int] not null,
    [CreatedDate] [datetime] not null,
    primary key ([FocusId])
);
create table [dbo].[FollowRequests] (
    [FollowRequestId] [int] not null identity,
    [FromUserId] [nvarchar](max) not null,
    [ToUserId] [nvarchar](max) not null,
    [Accepted] [bit] not null,
    primary key ([FollowRequestId])
);
create table [dbo].[FollowUsers] (
    [FollowUserId] [int] not null identity,
    [ToUserId] [nvarchar](128) null,
    [FromUserId] [nvarchar](128) null,
    [Accepted] [bit] not null,
    [AddedDate] [datetime] not null,
    [ApplicationUser_Id] [nvarchar](128) null,
    [ApplicationUser_Id1] [nvarchar](128) null,
    primary key ([FollowUserId])
);
create table [dbo].[Goals] (
    [GoalId] [int] not null identity,
    [GoalName] [nvarchar](55) not null,
    [Desc] [nvarchar](100) null,
    [StartDate] [datetime] not null,
    [EndDate] [datetime] not null,
    [Target] [float] null,
    [GoalType] [bit] not null,
    [MetricId] [int] null,
    [GoalStatusId] [int] not null,
    [UserId] [nvarchar](128) null,
    [CreatedDate] [datetime] not null,
    primary key ([GoalId])
);
create table [dbo].[GoalStatus] (
    [GoalStatusId] [int] not null identity,
    [GoalStatusType] [nvarchar](50) null,
    primary key ([GoalStatusId])
);
create table [dbo].[Groups] (
    [GroupId] [int] not null identity,
    [GroupName] [nvarchar](50) null,
    [Description] [nvarchar](max) null,
    [CreatedDate] [datetime] not null,
    primary key ([GroupId])
);
create table [dbo].[GroupComments] (
    [GroupCommentId] [int] not null identity,
    [CommentText] [nvarchar](max) null,
    [GroupUpdateId] [int] not null,
    [CommentDate] [datetime] not null,
    primary key ([GroupCommentId])
);
create table [dbo].[GroupCommentUsers] (
    [GroupCommentUserId] [int] not null identity,
    [GroupCommentId] [int] not null,
    [UserId] [nvarchar](max) not null,
    primary key ([GroupCommentUserId])
);
create table [dbo].[GroupGoals] (
    [GroupGoalId] [int] not null identity,
    [GoalName] [nvarchar](50) null,
    [Description] [nvarchar](100) null,
    [StartDate] [datetime] not null,
    [EndDate] [datetime] not null,
    [Target] [float] null,
    [MetricId] [int] null,
    [FocusId] [int] null,
    [CreatedDate] [datetime] not null,
    [GoalStatusId] [int] not null,
    [GroupUserId] [int] not null,
    [AssignedGroupUserId] [int] null,
    [AssignedTo] [nvarchar](max) null,
    [GroupId] [int] not null,
    primary key ([GroupGoalId])
);
create table [dbo].[GroupInvitations] (
    [GroupInvitationId] [int] not null identity,
    [FromUserId] [nvarchar](max) null,
    [GroupId] [int] not null,
    [ToUserId] [nvarchar](max) null,
    [SentDate] [datetime] not null,
    [Accepted] [bit] not null,
    primary key ([GroupInvitationId])
);
create table [dbo].[GroupRequests] (
    [GroupRequestId] [int] not null identity,
    [GroupId] [int] not null,
    [UserId] [nvarchar](128) null,
    [Accepted] [bit] not null,
    primary key ([GroupRequestId])
);
create table [dbo].[GroupUpdates] (
    [GroupUpdateId] [int] not null identity,
    [Updatemsg] [nvarchar](max) null,
    [status] [float] null,
    [GroupGoalId] [int] not null,
    [UpdateDate] [datetime] not null,
    primary key ([GroupUpdateId])
);
create table [dbo].[GroupUpdateSupports] (
    [GroupUpdateSupportId] [int] not null identity,
    [GroupUpdateId] [int] not null,
    [GroupUserId] [int] not null,
    [UpdateSupportedDate] [datetime] not null,
    primary key ([GroupUpdateSupportId])
);
create table [dbo].[GroupUpdateUsers] (
    [GroupUpdateUserId] [int] not null identity,
    [GroupUpdateId] [int] not null,
    [UserId] [nvarchar](max) not null,
    primary key ([GroupUpdateUserId])
);
create table [dbo].[GroupUsers] (
    [GroupUserId] [int] not null identity,
    [GroupId] [int] not null,
    [UserId] [nvarchar](max) not null,
    [Admin] [bit] not null,
    [AddedDate] [datetime] not null,
    primary key ([GroupUserId])
);
create table [dbo].[AspNetRoles] (
    [Id] [nvarchar](128) not null,
    [Name] [nvarchar](max) not null,
    primary key ([Id])
);
create table [dbo].[AspNetUsers] (
    [Id] [nvarchar](128) not null,
    [UserName] [nvarchar](max) not null,
    [PasswordHash] [nvarchar](max) null,
    [SecurityStamp] [nvarchar](max) null,
    [Email] [nvarchar](max) null,
    [FirstName] [nvarchar](max) null,
    [LastName] [nvarchar](max) null,
    [ProfilePicUrl] [nvarchar](max) null,
    [DateCreated] [datetime] null,
    [LastLoginTime] [datetime] null,
    [Activated] [bit] null,
    [RoleId] [int] null,
    [Discriminator] [nvarchar](128) not null,
    primary key ([Id])
);
create table [dbo].[AspNetUserClaims] (
    [Id] [int] not null identity,
    [ClaimType] [nvarchar](max) null,
    [ClaimValue] [nvarchar](max) null,
    [User_Id] [nvarchar](128) not null,
    primary key ([Id])
);
create table [dbo].[AspNetUserLogins] (
    [UserId] [nvarchar](128) not null,
    [LoginProvider] [nvarchar](128) not null,
    [ProviderKey] [nvarchar](128) not null,
    primary key ([UserId], [LoginProvider], [ProviderKey])
);
create table [dbo].[AspNetUserRoles] (
    [UserId] [nvarchar](128) not null,
    [RoleId] [nvarchar](128) not null,
    primary key ([UserId], [RoleId])
);
create table [dbo].[Metrics] (
    [MetricId] [int] not null identity,
    [Type] [nvarchar](max) null,
    primary key ([MetricId])
);
create table [dbo].[RegistrationTokens] (
    [RegistrationTokenId] [int] not null identity,
    [Token] [uniqueidentifier] not null,
    [Role] [nvarchar](50) null,
    primary key ([RegistrationTokenId])
);
create table [dbo].[SecurityTokens] (
    [SecurityTokenId] [int] not null identity,
    [Token] [uniqueidentifier] not null,
    [ActualID] [int] not null,
    primary key ([SecurityTokenId])
);
create table [dbo].[Supports] (
    [SupportId] [int] not null identity,
    [GoalId] [int] not null,
    [UserId] [nvarchar](max) null,
    [SupportedDate] [datetime] not null,
    primary key ([SupportId])
);
create table [dbo].[SupportInvitations] (
    [SupportInvitationId] [int] not null identity,
    [FromUserId] [nvarchar](max) null,
    [GoalId] [int] not null,
    [ToUserId] [nvarchar](max) null,
    [SentDate] [datetime] not null,
    [Accepted] [bit] not null,
    primary key ([SupportInvitationId])
);
create table [dbo].[Updates] (
    [UpdateId] [int] not null identity,
    [Updatemsg] [nvarchar](max) null,
    [status] [float] null,
    [GoalId] [int] not null,
    [UpdateDate] [datetime] not null,
    primary key ([UpdateId])
);
create table [dbo].[UpdateSupports] (
    [UpdateSupportId] [int] not null identity,
    [UpdateId] [int] not null,
    [UserId] [nvarchar](max) null,
    [UpdateSupportedDate] [datetime] not null,
    primary key ([UpdateSupportId])
);
create table [dbo].[UserProfiles] (
    [UserProfileId] [int] not null identity,
    [DateEdited] [datetime] not null,
    [Email] [nvarchar](max) null,
    [FirstName] [nvarchar](100) null,
    [LastName] [nvarchar](max) null,
    [DateOfBirth] [datetime] null,
    [Gender] [bit] null,
    [Address] [nvarchar](max) null,
    [City] [nvarchar](100) null,
    [State] [nvarchar](50) null,
    [Country] [nvarchar](50) null,
    [ZipCode] [float] null,
    [ContactNo] [float] null,
    [UserId] [nvarchar](50) null,
    primary key ([UserProfileId])
);
alter table [dbo].[FollowUsers] add constraint [ApplicationUser_FollowFromUser] foreign key ([ApplicationUser_Id]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[FollowUsers] add constraint [ApplicationUser_FollowToUser] foreign key ([ApplicationUser_Id1]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Goals] add constraint [ApplicationUser_Goals] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[GroupRequests] add constraint [ApplicationUser_GroupRequests] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[GroupGoals] add constraint [Focus_GroupGoals] foreign key ([FocusId]) references [dbo].[Foci]([FocusId]);
alter table [dbo].[FollowUsers] add constraint [FollowUser_FromUser] foreign key ([FromUserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[FollowUsers] add constraint [FollowUser_ToUser] foreign key ([ToUserId]) references [dbo].[AspNetUsers]([Id]);
alter table [dbo].[Updates] add constraint [Goal_Updates] foreign key ([GoalId]) references [dbo].[Goals]([GoalId]) on delete cascade;
alter table [dbo].[Goals] add constraint [GoalStatus_Goals] foreign key ([GoalStatusId]) references [dbo].[GoalStatus]([GoalStatusId]) on delete cascade;
alter table [dbo].[Foci] add constraint [Group_Foci] foreign key ([GroupId]) references [dbo].[Groups]([GroupId]) on delete cascade;
alter table [dbo].[GroupComments] add constraint [GroupComment_GroupUpdate] foreign key ([GroupUpdateId]) references [dbo].[GroupUpdates]([GroupUpdateId]) on delete cascade;
alter table [dbo].[GroupGoals] add constraint [GroupGoal_GoalStatus] foreign key ([GoalStatusId]) references [dbo].[GoalStatus]([GoalStatusId]) on delete cascade;
alter table [dbo].[GroupGoals] add constraint [GroupGoal_Group] foreign key ([GroupId]) references [dbo].[Groups]([GroupId]) on delete cascade;
alter table [dbo].[GroupInvitations] add constraint [GroupInvitation_Group] foreign key ([GroupId]) references [dbo].[Groups]([GroupId]) on delete cascade;
alter table [dbo].[GroupRequests] add constraint [GroupRequest_Group] foreign key ([GroupId]) references [dbo].[Groups]([GroupId]) on delete cascade;
alter table [dbo].[GroupUpdates] add constraint [GroupUpdate_GroupGoal] foreign key ([GroupGoalId]) references [dbo].[GroupGoals]([GroupGoalId]) on delete cascade;
alter table [dbo].[GroupUpdateSupports] add constraint [GroupUpdateSupport_GroupUpdate] foreign key ([GroupUpdateId]) references [dbo].[GroupUpdates]([GroupUpdateId]) on delete cascade;
alter table [dbo].[GroupGoals] add constraint [GroupUser_GroupGoals] foreign key ([GroupUserId]) references [dbo].[GroupUsers]([GroupUserId]) on delete cascade;
alter table [dbo].[AspNetUserClaims] add constraint [IdentityUserClaim_User] foreign key ([User_Id]) references [dbo].[AspNetUsers]([Id]) on delete cascade;
alter table [dbo].[AspNetUserLogins] add constraint [IdentityUserLogin_User] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]) on delete cascade;
alter table [dbo].[AspNetUserRoles] add constraint [IdentityUserRole_Role] foreign key ([RoleId]) references [dbo].[AspNetRoles]([Id]) on delete cascade;
alter table [dbo].[AspNetUserRoles] add constraint [IdentityUserRole_User] foreign key ([UserId]) references [dbo].[AspNetUsers]([Id]) on delete cascade;
alter table [dbo].[Goals] add constraint [Metric_Goals] foreign key ([MetricId]) references [dbo].[Metrics]([MetricId]);
alter table [dbo].[GroupGoals] add constraint [Metric_GroupGoals] foreign key ([MetricId]) references [dbo].[Metrics]([MetricId]);
alter table [dbo].[Supports] add constraint [Support_Goal] foreign key ([GoalId]) references [dbo].[Goals]([GoalId]) on delete cascade;
alter table [dbo].[SupportInvitations] add constraint [SupportInvitation_Goal] foreign key ([GoalId]) references [dbo].[Goals]([GoalId]) on delete cascade;
alter table [dbo].[Comments] add constraint [Update_Comments] foreign key ([UpdateId]) references [dbo].[Updates]([UpdateId]) on delete cascade;
alter table [dbo].[UpdateSupports] add constraint [UpdateSupport_Update] foreign key ([UpdateId]) references [dbo].[Updates]([UpdateId]) on delete cascade;
