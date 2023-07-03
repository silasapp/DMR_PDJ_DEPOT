CREATE VIEW [dbo].[vApplicationForms]
AS
SELECT        dbo.ApplicationForms.Id, dbo.ApplicationForms.FormId, dbo.ApplicationForms.ApplicationId, dbo.ApplicationForms.Filled, dbo.ApplicationForms.ExtraReport2, dbo.ApplicationForms.ExtraReport1, 
                         dbo.ApplicationForms.StaffName, dbo.ApplicationForms.ValGroupId, dbo.ApplicationForms.ManagerReason, dbo.ApplicationForms.ManagerAccept, dbo.ApplicationForms.DepartmentId, dbo.ApplicationForms.WaiverApproved, 
                         dbo.ApplicationForms.WaiverRequest, dbo.ApplicationForms.FormTitle, dbo.ApplicationForms.Reasons, dbo.ApplicationForms.Recommend, dbo.ApplicationForms.DateModified, dbo.ApplicationForms.Date, 
                         dbo.ApplicationForms.Confirmed, dbo.companies.name AS CompanyName, dbo.Facilities.Name AS FacilityName, dbo.applications.FacilityId, dbo.applications.company_id AS companyId
FROM            dbo.ApplicationForms INNER JOIN
                         dbo.applications ON dbo.applications.id = dbo.ApplicationForms.ApplicationId INNER JOIN
                         dbo.companies ON dbo.companies.id = dbo.applications.company_id INNER JOIN
                         dbo.Facilities ON dbo.Facilities.Id = dbo.applications.FacilityId