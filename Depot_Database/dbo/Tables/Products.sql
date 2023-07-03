CREATE TABLE [dbo].[Products] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (50) NOT NULL,
    [FriendlyName]  NVARCHAR (50) NOT NULL,
    [UpdatedAt]     DATETIME      NULL,
    [CreatedAt]     DATETIME      NULL,
    [DeletedStatus] BIT           NULL,
    [DeletedBy]     INT           NULL,
    [DeletedAt]     DATETIME      NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
);

