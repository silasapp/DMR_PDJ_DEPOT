CREATE TABLE [dbo].[ApplicationJobSpecPresentations] (
    [Id]                   INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId]        INT           NOT NULL,
    [Job_Specification_Id] NVARCHAR (50) NULL,
    [PresentationApproved] BIT           NULL,
    [InspectionApproved]   BIT           NULL,
    [DepartmentId]         INT           NULL,
    [PresentationRequired] BIT           NULL,
    [InspectionRequired]   BIT           NULL,
    CONSTRAINT [PK_ApplicationJobSpecPresentations] PRIMARY KEY CLUSTERED ([Id] ASC)
);

