CREATE TABLE [dbo].[MeetingSchedules] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Message]         NVARCHAR (400)  NULL,
    [Venue]           NVARCHAR (200)  NULL,
    [ApplicationId]   INT             NOT NULL,
    [StaffUserName]   NVARCHAR (200)  NOT NULL,
    [Address]         NVARCHAR (400)  NULL,
    [Date]            DATETIME        NOT NULL,
    [Accepted]        BIT             NULL,
    [Approved]        BIT             NULL,
    [ApprovedBy]      NVARCHAR (400)  NULL,
    [ApprovedDate]    DATETIME        NULL,
    [MeetingDate]     DATETIME        NOT NULL,
    [DeclineReason]   NVARCHAR (2000) NULL,
    [AcceptanceDate]  DATETIME        NULL,
    [WaiverRequest]   BIT             NULL,
    [WaiverReason]    NVARCHAR (1000) NULL,
    [ScheduleExpired] BIT             NULL,
    [Completed]       BIT             NULL,
    [FinalComment]    NVARCHAR (2000) NULL,
    [VenueId]         INT             NULL,
    [UpdatedAt]       DATETIME        NULL,
    [SchedulerID]     INT             NULL,
    CONSTRAINT [PK_MeetingSchedules] PRIMARY KEY CLUSTERED ([Id] ASC)
);



