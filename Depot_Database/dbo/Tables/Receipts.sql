CREATE TABLE [dbo].[Receipts] (
    [id]                   BIGINT         IDENTITY (1, 1) NOT NULL,
    [receiptno]            NVARCHAR (30)  NULL,
    [applicationid]        INT            NOT NULL,
    [invoiceid]            INT            NOT NULL,
    [companyname]          NVARCHAR (250) NOT NULL,
    [amount]               FLOAT (53)     NOT NULL,
    [status]               NVARCHAR (10)  NOT NULL,
    [applicationreference] NVARCHAR (15)  NOT NULL,
    [RRR]                  NVARCHAR (15)  NULL,
    [date_paid]            DATETIME       NULL,
    CONSTRAINT [PK_Receipts_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

