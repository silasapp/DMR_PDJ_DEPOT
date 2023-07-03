CREATE TABLE [dbo].[invoices] (
    [id]                    BIGINT        IDENTITY (1, 1) NOT NULL,
    [application_id]        INT           NOT NULL,
    [amount]                FLOAT (53)    NOT NULL,
    [status]                VARCHAR (6)   NOT NULL,
    [payment_code]          CHAR (100)    NOT NULL,
    [payment_type]          CHAR (50)     NOT NULL,
    [receipt_no]            NVARCHAR (20) NULL,
    [date_added]            DATETIME      NULL,
    [date_paid]             DATETIME      NULL,
    [PaymentTransaction_Id] INT           NULL,
    CONSTRAINT [PK_invoice_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

