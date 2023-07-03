CREATE TABLE [dbo].[InspectionMeetingAttendees] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [MeetingScheduleId] INT            NOT NULL,
    [StaffEmail]        NVARCHAR (250) NOT NULL,
    [Date]              DATETIME       NOT NULL,
    CONSTRAINT [PK_InspectionMeetingAttendees] PRIMARY KEY CLUSTERED ([Id] ASC)
);

