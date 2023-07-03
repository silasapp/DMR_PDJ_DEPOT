CREATE TABLE [dbo].[payment_transactions] (
    [id]                   INT           IDENTITY (7601, 1) NOT NULL,
    [type]                 VARCHAR (50)  NOT NULL,
    [transaction_date]     VARCHAR (50)  NOT NULL,
    [reference_number]     VARCHAR (50)  NULL,
    [online_reference]     VARCHAR (50)  NULL,
    [payment_reference]    VARCHAR (100) NULL,
    [approved_amount]      VARCHAR (10)  NULL,
    [response_description] VARCHAR (100) NULL,
    [response_code]        VARCHAR (50)  NULL,
    [transaction_amount]   VARCHAR (10)  NULL,
    [transaction_currency] VARCHAR (5)   NULL,
    [customer_name]        VARCHAR (200) NULL,
    [customer_id]          INT           NOT NULL,
    [order_id]             VARCHAR (20)  NOT NULL,
    [payment_log_id]       VARCHAR (50)  NULL,
    [query_date]           DATETIME2 (0) NOT NULL,
    [Webpay_Reference]     NVARCHAR (50) NULL,
    [RRR]                  NVARCHAR (50) NULL,
    [PaymentSource]        NVARCHAR (50) NULL,
    CONSTRAINT [PK_payment_transaction_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

