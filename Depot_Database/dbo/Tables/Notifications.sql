CREATE TABLE [dbo].[Notifications] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [ToStaff]     NVARCHAR (250)  NULL,
    [Message]     NVARCHAR (2000) NULL,
    [DateAdded]   DATETIME        NULL,
    [IsRead]      BIT             NULL,
    [DateRead]    DATETIME        NULL,
    [Deleted]     BIT             NULL,
    [DateDeleted] DATETIME        NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC)
);

