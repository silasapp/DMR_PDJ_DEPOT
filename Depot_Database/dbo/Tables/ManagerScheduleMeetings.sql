CREATE TABLE [dbo].[ManagerScheduleMeetings] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (200) NOT NULL,
    [ScheduleId] INT            NOT NULL,
    CONSTRAINT [PK_ManagerScheduleMessages] PRIMARY KEY CLUSTERED ([Id] ASC)
);

