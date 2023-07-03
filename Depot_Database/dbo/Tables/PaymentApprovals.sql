CREATE TABLE [dbo].[PaymentApprovals] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [ApplicationId] INT           NOT NULL,
    [CompanyId]     INT           NOT NULL,
    [PaymentId]     VARCHAR (150) NOT NULL,
    [Bank]          VARCHAR (150) NOT NULL,
    [PaymentUrl]    VARCHAR (450) NOT NULL,
    [UserName]      VARCHAR (250) NOT NULL,
    [Amount]        FLOAT (53)    NULL,
    [Status]        VARCHAR (150) NOT NULL,
    [Comment]       VARCHAR (450) NULL,
    [Date]          DATETIME      NOT NULL,
    CONSTRAINT [PK_PaymentApprovals] PRIMARY KEY CLUSTERED ([Id] ASC)
);

