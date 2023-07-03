


CREATE VIEW [dbo].[vPermits]
AS
SELECT        dbo.permits.id, dbo.permits.permit_no, dbo.companies.name AS CompanyName, dbo.permits.application_id, dbo.permits.date_issued, dbo.permits.date_expire, dbo.permits.company_id, dbo.applications.fee_payable AS Fees,
                             (SELECT        name
                               FROM            dbo.Categories
                               WHERE        (id = dbo.applications.category_id)) AS CategoryName, dbo.permits.elps_id, dbo.applications.reference AS OrderId, dbo.applications.PhaseId, dbo.Phases.name AS PhaseName, dbo.applications.FacilityId, 
                         dbo.applications.category_id, dbo.permits.is_renewed, dbo.vFacilities.Name AS FacilityName, dbo.vFacilities.address_1 AS FacilityAddress, dbo.permits.Printed, dbo.vFacilities.StateName, dbo.Phases.IssueType
FROM            dbo.permits INNER JOIN
                         dbo.companies ON dbo.permits.company_id = dbo.companies.id INNER JOIN
                         dbo.applications ON dbo.permits.application_id = dbo.applications.id INNER JOIN
                         dbo.Phases ON dbo.applications.PhaseId = dbo.Phases.id INNER JOIN
                         dbo.vFacilities ON dbo.applications.FacilityId = dbo.vFacilities.Id