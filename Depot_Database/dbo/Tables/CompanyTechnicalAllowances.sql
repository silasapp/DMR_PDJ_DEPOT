CREATE TABLE [dbo].[CompanyTechnicalAllowances] (
    [CompanyId] INT        NOT NULL,
    [Quantity]  FLOAT (53) NOT NULL,
    CONSTRAINT [PK_CompanyTechnicalAllowances] PRIMARY KEY CLUSTERED ([CompanyId] ASC)
);

