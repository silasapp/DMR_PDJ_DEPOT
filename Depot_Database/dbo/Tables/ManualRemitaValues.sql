CREATE TABLE [dbo].[ManualRemitaValues] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [RRR]           NVARCHAR (15)  NULL,
    [Company]       NVARCHAR (250) NULL,
    [Beneficiary]   NVARCHAR (250) NULL,
    [FundingBank]   NVARCHAR (150) NULL,
    [PaymentSource] NVARCHAR (150) NULL,
    [NetAmount]     FLOAT (53)     NULL,
    [Status]        NVARCHAR (50)  NULL,
    [DateAdded]     DATETIME       NOT NULL,
    [AddedBy]       NVARCHAR (250) NULL,
    CONSTRAINT [PK_ManualRemitaValues] PRIMARY KEY CLUSTERED ([Id] ASC)
);

