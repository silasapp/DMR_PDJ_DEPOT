CREATE TABLE [dbo].[FacilityConditions] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [NumberOfAttendants]     INT            NOT NULL,
    [AttentdantsHaveUniform] BIT            NOT NULL,
    [HasABackOffice]         BIT            NOT NULL,
    [DPRStaff]               NVARCHAR (150) NOT NULL,
    [Date]                   DATETIME       NOT NULL,
    [ApplicationId]          INT            NOT NULL,
    [FacilityId]             INT            NOT NULL,
    CONSTRAINT [PK_FacilityConditions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

