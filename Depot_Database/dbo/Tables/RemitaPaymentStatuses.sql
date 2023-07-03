CREATE TABLE [dbo].[RemitaPaymentStatuses] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [rrr]              NVARCHAR (50) NULL,
    [channnel]         NVARCHAR (50) NULL,
    [amount]           NVARCHAR (50) NULL,
    [responseCode]     NVARCHAR (50) NULL,
    [transactiondate]  NVARCHAR (50) NULL,
    [bank]             NVARCHAR (50) NULL,
    [branch]           NVARCHAR (50) NULL,
    [serviceTypeId]    NVARCHAR (50) NULL,
    [datesent]         NVARCHAR (50) NULL,
    [daterequested]    NVARCHAR (50) NULL,
    [OrderRef]         NVARCHAR (50) NULL,
    [PayerName]        NVARCHAR (50) NULL,
    [PayerEmail]       NVARCHAR (50) NULL,
    [PayerPhoneNumber] NVARCHAR (50) NULL,
    [uniqueidentifier] NVARCHAR (50) NULL,
    [debitdate]        NVARCHAR (50) NULL,
    CONSTRAINT [PK_RemitaPaymentStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);

