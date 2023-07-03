CREATE TABLE [dbo].[JointAccounts] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [ApplicationId]       INT            NOT NULL,
    [Opscon]              NVARCHAR (250) NOT NULL,
    [DateAdded]           DATETIME       NULL,
    [OperationsCompleted] BIT            NULL,
    [Assigned]            BIT            NULL,
    CONSTRAINT [PK_JointAccounts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

