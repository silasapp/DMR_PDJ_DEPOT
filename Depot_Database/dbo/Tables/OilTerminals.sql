CREATE TABLE [dbo].[OilTerminals] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (250) NOT NULL,
    [Type]            NVARCHAR (50)  NOT NULL,
    [Platform]        NVARCHAR (50)  NULL,
    [Product]         NVARCHAR (50)  NOT NULL,
    [OperationalZone] NVARCHAR (50)  NULL,
    [SupervisedBy]    NVARCHAR (50)  NULL,
    CONSTRAINT [PK_OilTerminals] PRIMARY KEY CLUSTERED ([Id] ASC)
);

