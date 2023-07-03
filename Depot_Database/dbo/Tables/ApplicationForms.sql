CREATE TABLE [dbo].[ApplicationForms] (
    [Id]                     INT              IDENTITY (1, 1) NOT NULL,
    [FormId]                 NVARCHAR (150)   NOT NULL,
    [ApplicationId]          INT              NOT NULL,
    [Confirmed]              BIT              NOT NULL,
    [Date]                   DATETIME         NOT NULL,
    [InspectionScheduleDate] DATETIME         NULL,
    [DateModified]           DATETIME         NOT NULL,
    [Filled]                 BIT              NOT NULL,
    [Recommend]              BIT              NULL,
    [Reasons]                NVARCHAR (MAX)   NULL,
    [FormTitle]              NVARCHAR (250)   NULL,
    [WaiverRequest]          BIT              NULL,
    [WaiverApproved]         BIT              NULL,
    [DepartmentId]           INT              NULL,
    [ManagerAccept]          BIT              NULL,
    [ManagerReason]          NVARCHAR (MAX)   NULL,
    [ValGroupId]             UNIQUEIDENTIFIER NULL,
    [StaffName]              NVARCHAR (250)   NULL,
    [ExtraReport1]           NVARCHAR (1000)  NULL,
    [ExtraReport2]           NVARCHAR (1000)  NULL,
    CONSTRAINT [PK_ApplicationForms] PRIMARY KEY CLUSTERED ([Id] ASC)
);

