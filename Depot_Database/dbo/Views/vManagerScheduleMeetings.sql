CREATE VIEW [dbo].[vManagerScheduleMeetings]
AS
SELECT        dbo.MeetingSchedules.Id, dbo.MeetingSchedules.Message, dbo.MeetingSchedules.Venue, dbo.MeetingSchedules.ApplicationId, dbo.MeetingSchedules.StaffUserName, dbo.MeetingSchedules.Address, 
                         dbo.MeetingSchedules.Date, dbo.MeetingSchedules.Accepted, dbo.MeetingSchedules.Approved, dbo.MeetingSchedules.ApprovedDate, dbo.ManagerScheduleMeetings.UserId, 
                         dbo.MeetingSchedules.MeetingDate, dbo.MeetingSchedules.WaiverRequest, dbo.MeetingSchedules.WaiverReason, dbo.MeetingSchedules.AcceptanceDate, dbo.MeetingSchedules.ScheduleExpired
FROM            dbo.MeetingSchedules INNER JOIN
                         dbo.ManagerScheduleMeetings ON dbo.MeetingSchedules.Id = dbo.ManagerScheduleMeetings.ScheduleId