CREATE TABLE [dbo].[Logs] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [UserId] NVARCHAR (MAX) NULL,
    [Action] NVARCHAR (MAX) NULL,
    [Error]  NVARCHAR (MAX) NULL,
    [Date]   DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

