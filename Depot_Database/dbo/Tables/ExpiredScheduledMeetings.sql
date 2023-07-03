CREATE TABLE [dbo].[ExpiredScheduledMeetings] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [ScheduleId] INT      NOT NULL,
    [Date]       DATETIME NOT NULL,
    CONSTRAINT [PK_ExpiredScheduledMeetings] PRIMARY KEY CLUSTERED ([Id] ASC)
);

