CREATE TABLE [dbo].[Leaves] (
    [Id]                   INT              IDENTITY (1, 1) NOT NULL,
    [ApplicantId]          UNIQUEIDENTIFIER NOT NULL,
    [ManagerId]            UNIQUEIDENTIFIER NOT NULL,
    [ManagerApproved]      BIT              NULL,
    [DateStart]            DATETIME         NOT NULL,
    [DateEnd]              DATETIME         NOT NULL,
    [Ended]                BIT              NULL,
    [Reason]               NVARCHAR (MAX)   NULL,
    [Started]              BIT              NULL,
    [DateLogged]           DATETIME         NULL,
    [ReasonForDisapproval] NVARCHAR (2000)  NULL,
    CONSTRAINT [PK_Leaves] PRIMARY KEY CLUSTERED ([Id] ASC)
);

