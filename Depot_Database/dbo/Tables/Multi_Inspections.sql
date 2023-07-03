CREATE TABLE [dbo].[Multi_Inspections] (
    [Id]                 INT IDENTITY (1, 1) NOT NULL,
    [ApplicationId]      INT NULL,
    [InspectionApproved] BIT NULL,
    [DepartmentId]       INT NULL,
    CONSTRAINT [PK_Multi_Inspections] PRIMARY KEY CLUSTERED ([Id] ASC)
);

