CREATE TABLE [dbo].[Forms] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [TableName]       NVARCHAR (50) NOT NULL,
    [FriendlyName]    NVARCHAR (50) NULL,
    [IsPublished]     BIT           NULL,
    [Deleted]         BIT           NULL,
    [CreatedByUserId] NVARCHAR (50) NULL,
    [Title]           NVARCHAR (50) NULL,
    [CreatedOnDate]   DATETIME      NOT NULL,
    [PhaseId]         INT           NULL,
    [OtherPhases]     VARCHAR (10)  NULL,
    CONSTRAINT [PK_Forms] PRIMARY KEY CLUSTERED ([Id] ASC)
);

