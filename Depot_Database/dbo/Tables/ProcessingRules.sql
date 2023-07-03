CREATE TABLE [dbo].[ProcessingRules] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [DepartmentId] INT NOT NULL,
    [RoleId]       INT NOT NULL,
    CONSTRAINT [PK_Inspection_Rules] PRIMARY KEY CLUSTERED ([Id] ASC)
);

