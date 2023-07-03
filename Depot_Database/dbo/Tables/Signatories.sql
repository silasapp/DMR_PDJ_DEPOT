CREATE TABLE [dbo].[Signatories] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      VARCHAR (250) NOT NULL,
    [Position]  VARCHAR (50)  NOT NULL,
    [Signature] VARCHAR (250) NOT NULL,
    [StartDate] DATETIME      NOT NULL,
    [EndDate]   DATETIME      NULL,
    [Type]      VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Signatories] PRIMARY KEY CLUSTERED ([Id] ASC)
);

