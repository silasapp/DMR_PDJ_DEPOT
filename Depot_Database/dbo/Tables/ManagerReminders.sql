CREATE TABLE [dbo].[ManagerReminders] (
    [Id]                   UNIQUEIDENTIFIER NOT NULL,
    [DateAdded]            DATETIME         NULL,
    [ReminderSent]         BIT              NOT NULL,
    [ScheduleId]           INT              NOT NULL,
    [InspectionScheduleId] INT              NULL,
    CONSTRAINT [PK_ManagerReminders] PRIMARY KEY CLUSTERED ([Id] ASC)
);

