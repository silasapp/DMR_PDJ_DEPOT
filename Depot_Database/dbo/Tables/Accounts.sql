CREATE TABLE [dbo].[Accounts] (
    [ID]          INT           IDENTITY (1, 1) NOT NULL,
    [BankName]    NVARCHAR (50) NULL,
    [BankAccNo]   NVARCHAR (10) NULL,
    [BankAccName] NVARCHAR (50) NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED ([ID] ASC)
);

