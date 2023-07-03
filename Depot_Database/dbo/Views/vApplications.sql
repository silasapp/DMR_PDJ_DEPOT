CREATE VIEW [dbo].[vApplications]
AS
SELECT        dbo.applications.id, dbo.applications.company_id, dbo.applications.category_id, dbo.applications.type, dbo.applications.year, dbo.applications.fee_payable, dbo.applications.service_charge, dbo.applications.status, 
                         dbo.applications.date_added, dbo.applications.date_modified, dbo.Categories.name AS CategoryName, dbo.companies.name AS companyName, dbo.applications.reference, dbo.applications.current_desk, 
                         dbo.companies.user_id, dbo.applications.submitted, dbo.applications.PhaseId, dbo.Phases.name AS PhaseName, dbo.Facilities.Name AS FacilityName, dbo.Facilities.AddressId, dbo.vAddresses.address_1, 
                         dbo.vAddresses.city, dbo.vAddresses.StateName, dbo.vAddresses.CountryName, dbo.applications.FacilityId, dbo.applications.AllowPush, dbo.Phases.ShortName AS PhaseShortName, dbo.applications.AppProcessed, 
                         dbo.applications.SupervisorProcessed, dbo.applications.current_Permit, dbo.applications.PaymentDescription, dbo.Phases.IssueType, dbo.Facilities.IdentificationCode AS FacIdentificationCode, 
                         dbo.Facilities.CategoryCode
FROM            dbo.applications INNER JOIN
                         dbo.companies ON dbo.applications.company_id = dbo.companies.id INNER JOIN
                         dbo.Categories ON dbo.applications.category_id = dbo.Categories.id INNER JOIN
                         dbo.Phases ON dbo.applications.PhaseId = dbo.Phases.id INNER JOIN
                         dbo.Facilities ON dbo.applications.FacilityId = dbo.Facilities.Id INNER JOIN
                         dbo.vAddresses ON dbo.Facilities.AddressId = dbo.vAddresses.id