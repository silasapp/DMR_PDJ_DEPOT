CREATE VIEW [dbo].[vInspectionScheduleApplications]
AS
SELECT        dbo.InspectionSchedules.Id, dbo.InspectionSchedules.CompanyId, dbo.InspectionSchedules.Message, dbo.InspectionSchedules.Venue, dbo.InspectionSchedules.ApplicationId, 
                         dbo.InspectionSchedules.StaffUserName, dbo.InspectionSchedules.Address, dbo.InspectionSchedules.MeetingDate, dbo.InspectionSchedules.DateAdded, dbo.InspectionSchedules.Accepted, 
                         dbo.InspectionSchedules.DeclineReason, dbo.InspectionSchedules.AcceptanceDate, dbo.InspectionSchedules.ScheduleExpired, dbo.vApplications.companyName, dbo.vApplications.reference
FROM            dbo.InspectionSchedules INNER JOIN
                         dbo.vApplications ON dbo.InspectionSchedules.ApplicationId = dbo.vApplications.id