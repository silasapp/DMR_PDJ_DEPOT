CREATE TABLE [dbo].[ATCCompanies] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [ATCId]    INT            NOT NULL,
    [Name]     NVARCHAR (200) NOT NULL,
    [RCNumber] NVARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_ATCCompanies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

