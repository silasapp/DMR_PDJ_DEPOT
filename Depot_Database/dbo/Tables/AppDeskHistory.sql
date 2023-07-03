CREATE TABLE [dbo].[AppDeskHistory] (
    [HistoryID] INT           IDENTITY (1, 1) NOT NULL,
    [RequestID] INT           NOT NULL,
    [StaffID]   INT           NOT NULL,
    [Status]    VARCHAR (20)  NOT NULL,
    [Comment]   VARCHAR (MAX) NOT NULL,
    [CreatedAt] DATETIME      NOT NULL,
    CONSTRAINT [PK_AppDeskHistory] PRIMARY KEY CLUSTERED ([HistoryID] ASC)
);

