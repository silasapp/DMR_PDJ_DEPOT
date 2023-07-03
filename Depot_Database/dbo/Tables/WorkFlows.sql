CREATE TABLE [dbo].[WorkFlows] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [OfficeLocation]    NVARCHAR (MAX) NULL,
    [Action]            NVARCHAR (MAX) NULL,
    [TriggeredByRole]   NVARCHAR (MAX) NULL,
    [TargetRole]        NVARCHAR (MAX) NULL,
    [Status]            NVARCHAR (MAX) NULL,
    [State]             NVARCHAR (MAX) NULL,
    [ApplicationTypeId] INT            NULL,
    [CategoryId]        INT            CONSTRAINT [DF__WorkFlows__Categ__1699586C] DEFAULT ((0)) NOT NULL,
    [ProductId]         INT            CONSTRAINT [DF__WorkFlows__Produ__178D7CA5] DEFAULT ((0)) NOT NULL,
    [IsActive]          BIT            CONSTRAINT [DF__WorkFlows__IsAct__1881A0DE] DEFAULT (CONVERT([bit],(0))) NULL,
    CONSTRAINT [PK_WorkFlows] PRIMARY KEY CLUSTERED ([Id] ASC)
);

