CREATE VIEW [dbo].[FacilityPermits]
AS
SELECT        dbo.permits.permit_no, dbo.permits.date_issued, dbo.permits.date_expire, dbo.permits.is_renewed, dbo.permits.elps_id, dbo.permits.id, dbo.vApplications.company_id AS CompanyId, dbo.vApplications.type, 
                         dbo.vApplications.FacilityName, dbo.vApplications.PhaseName, dbo.vApplications.PhaseId, dbo.vApplications.companyName, dbo.vApplications.CategoryName, dbo.permits.application_id, 
                         dbo.vApplications.category_id, dbo.vApplications.FacilityId
FROM            dbo.permits INNER JOIN
                         dbo.vApplications ON dbo.permits.application_id = dbo.vApplications.id