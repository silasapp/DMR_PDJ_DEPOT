CREATE TABLE [dbo].[UserBranches] (
    [Id]           INT            NOT NULL,
    [UserEmail]    NVARCHAR (128) NOT NULL,
    [BranchId]     INT            NOT NULL,
    [RoleId]       INT            NOT NULL,
    [DepartmentId] INT            NOT NULL,
    [DeskCount]    INT            NULL,
    [Active]       BIT            NULL
);



