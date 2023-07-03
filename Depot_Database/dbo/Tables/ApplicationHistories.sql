CREATE TABLE [dbo].[ApplicationHistories] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [ApplicationId]      INT            NOT NULL,
    [DateAssigned]       DATETIME2 (7)  NOT NULL,
    [WorkFlowId]         INT            NOT NULL,
    [Comment]            NVARCHAR (MAX) NULL,
    [Status]             NVARCHAR (MAX) NULL,
    [IsAssigned]         BIT            NOT NULL,
    [Action]             NVARCHAR (MAX) NULL,
    [CurrentUserRole]    NVARCHAR (MAX) NULL,
    [CurrentUser]        NVARCHAR (MAX) NULL,
    [ProcessingUserRole] NVARCHAR (MAX) NULL,
    [ProcessingUser]     NVARCHAR (MAX) NULL,
    [AutoPushed]         BIT            NOT NULL,
    [DateTreated]        DATETIME2 (7)  NULL,
    [ReceivedFieldId]    NVARCHAR (MAX) NULL,
    [SentFieldId]        NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ApplicationHistories] PRIMARY KEY CLUSTERED ([Id] ASC)
);

