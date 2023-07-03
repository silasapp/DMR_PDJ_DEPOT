CREATE TABLE [dbo].[reversal_transactions] (
    [id]                         INT           IDENTITY (1, 1) NOT NULL,
    [transaction_date]           VARCHAR (50)  NOT NULL,
    [reference_number]           VARCHAR (50)  NOT NULL,
    [payment_reference]          VARCHAR (50)  NOT NULL,
    [approved_amount]            VARCHAR (10)  NOT NULL,
    [response_description]       VARCHAR (100) NOT NULL,
    [response_code]              VARCHAR (50)  NOT NULL,
    [transaction_amount]         VARCHAR (10)  NOT NULL,
    [transaction_currency]       VARCHAR (5)   NOT NULL,
    [customer_name]              VARCHAR (50)  NOT NULL,
    [customer_id]                INT           NOT NULL,
    [order_id]                   VARCHAR (20)  NOT NULL,
    [payment_log_id]             VARCHAR (100) NOT NULL,
    [original_payment_reference] VARCHAR (100) NOT NULL,
    [original_payment_log_id]    VARCHAR (100) NOT NULL,
    [query_date]                 DATETIME2 (0) NOT NULL,
    CONSTRAINT [PK_reversal_transaction_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

