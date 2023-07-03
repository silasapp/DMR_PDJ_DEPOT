CREATE TABLE [dbo].[RunTimes] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [LastRunTime]     DATETIME       NULL,
    [ResponseMessage] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_RunTimes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

