CREATE TABLE [dbo].[currencies] (
    [id]            INT             IDENTITY (1, 1) NOT NULL,
    [title]         NVARCHAR (32)   NOT NULL,
    [code]          NVARCHAR (3)    NOT NULL,
    [symbol_left]   NVARCHAR (12)   NOT NULL,
    [symbol_right]  NVARCHAR (12)   NOT NULL,
    [decimal_place] NCHAR (1)       NOT NULL,
    [value]         NUMERIC (15, 8) NOT NULL,
    [status]        SMALLINT        NOT NULL,
    [date_modified] DATETIME2 (0)   NOT NULL,
    CONSTRAINT [PK_currency_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

