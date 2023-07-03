CREATE VIEW [dbo].[vMeetingScheduleApplications]
AS
SELECT        dbo.vManagerScheduleMeetings.Id, dbo.vManagerScheduleMeetings.Message, dbo.vManagerScheduleMeetings.Venue, dbo.vManagerScheduleMeetings.ApplicationId, 
                         dbo.vManagerScheduleMeetings.StaffUserName, dbo.vManagerScheduleMeetings.Address, dbo.vManagerScheduleMeetings.Date, dbo.vManagerScheduleMeetings.Accepted, 
                         dbo.vManagerScheduleMeetings.Approved, dbo.vManagerScheduleMeetings.ApprovedDate, dbo.vManagerScheduleMeetings.UserId, dbo.vApplications.company_id, dbo.vApplications.reference, 
                         dbo.vApplications.companyName, dbo.vApplications.user_id AS AppUserName, dbo.vApplications.CategoryName, dbo.vApplications.year, dbo.vManagerScheduleMeetings.MeetingDate, 
                         dbo.vManagerScheduleMeetings.WaiverRequest, dbo.vManagerScheduleMeetings.AcceptanceDate, dbo.vManagerScheduleMeetings.ScheduleExpired
FROM            dbo.vManagerScheduleMeetings INNER JOIN
                         dbo.vApplications ON dbo.vManagerScheduleMeetings.ApplicationId = dbo.vApplications.id