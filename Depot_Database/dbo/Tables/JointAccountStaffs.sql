CREATE TABLE [dbo].[JointAccountStaffs] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [JointAccountId]      INT            NOT NULL,
    [Staff]               NVARCHAR (250) NULL,
    [DateAdded]           DATETIME       NULL,
    [SignedOff]           BIT            NULL,
    [OperationsCompleted] BIT            NULL,
    [ApplicationId]       INT            NULL,
    CONSTRAINT [PK_JointAccountStaffs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

