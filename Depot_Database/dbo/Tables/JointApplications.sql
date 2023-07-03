CREATE TABLE [dbo].[JointApplications] (
    [id]                  INT            IDENTITY (1, 1) NOT NULL,
    [JointAccountId]      INT            NOT NULL,
    [applicationId]       INT            NOT NULL,
    [FacilityId]          INT            NOT NULL,
    [Staff]               NVARCHAR (100) NULL,
    [FacilityName]        NVARCHAR (120) NULL,
    [PhaseName]           NVARCHAR (50)  NULL,
    [Opscon]              NVARCHAR (100) NULL,
    [DateAdded]           DATETIME       NULL,
    [UpdatedAt]           DATETIME       NULL,
    [SignedOff]           BIT            NULL,
    [Assigned]            BIT            NULL,
    [AllowPAush]          BIT            NULL,
    [OperationsCompleted] BIT            NULL,
    CONSTRAINT [PK_JointAccountApplications] PRIMARY KEY CLUSTERED ([id] ASC)
);

