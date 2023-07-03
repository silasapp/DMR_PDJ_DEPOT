CREATE TABLE [dbo].[WorkRoles] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_WorkRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);

