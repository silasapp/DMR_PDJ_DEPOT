CREATE TABLE [dbo].[ApplicationExtraPayments] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [ApplicationId] INT             NOT NULL,
    [Type]          NVARCHAR (50)   NULL,
    [Amount]        DECIMAL (18, 2) NOT NULL,
    [RRR]           NVARCHAR (50)   NULL,
    [Reference]     NVARCHAR (50)   NOT NULL,
    [Comment]       NVARCHAR (MAX)  NULL,
    [Date]          DATETIME        NULL,
    [UserName]      NVARCHAR (250)  NULL,
    [Status]        NVARCHAR (50)   NULL,
    [DatePaid]      NVARCHAR (150)  NULL,
    CONSTRAINT [PK_ApplicationExtraPayments] PRIMARY KEY CLUSTERED ([Id] ASC)
);

