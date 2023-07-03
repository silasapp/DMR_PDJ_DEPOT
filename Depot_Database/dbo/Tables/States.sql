CREATE TABLE [dbo].[States] (
    [Id]        INT            IDENTITY (38, 1) NOT NULL,
    [CountryId] INT            NOT NULL,
    [Name]      NVARCHAR (100) NOT NULL,
    [Code]      NVARCHAR (10)  NULL,
    CONSTRAINT [PK_StatesS] PRIMARY KEY CLUSTERED ([Id] ASC)
);



