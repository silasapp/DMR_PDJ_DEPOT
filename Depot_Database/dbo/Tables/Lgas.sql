CREATE TABLE [dbo].[Lgas] (
    [Id]      INT           IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (50) NOT NULL,
    [StateId] INT           NOT NULL,
    [LGA_Code] CHAR(10) NULL, 
    CONSTRAINT [PK_Lgas] PRIMARY KEY CLUSTERED ([Id] ASC)
);

