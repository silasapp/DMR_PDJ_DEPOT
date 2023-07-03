CREATE TABLE [dbo].[InspectionSchedules] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [CompanyId]       INT             NULL,
    [Message]         NVARCHAR (4000) NULL,
    [Venue]           NVARCHAR (200)  NULL,
    [ApplicationId]   INT             NOT NULL,
    [StaffUserName]   NVARCHAR (200)  NOT NULL,
    [Address]         NVARCHAR (400)  NULL,
    [MeetingDate]     DATETIME        NOT NULL,
    [DateAdded]       DATETIME        NOT NULL,
    [Accepted]        BIT             NULL,
    [AcceptanceDate]  DATETIME        NULL,
    [DeclineReason]   NVARCHAR (2000) NULL,
    [ScheduleExpired] BIT             NOT NULL,
    CONSTRAINT [PK_InspectionSchedule] PRIMARY KEY CLUSTERED ([Id] ASC)
);

