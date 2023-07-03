CREATE VIEW [dbo].[vJointApplications]
AS
SELECT        dbo.vApplications.id, dbo.vApplications.company_id, dbo.vApplications.category_id, dbo.vApplications.type, dbo.vApplications.year, dbo.vApplications.fee_payable, dbo.vApplications.service_charge, dbo.vApplications.status, 
                         dbo.vApplications.date_added, dbo.vApplications.date_modified, dbo.vApplications.CategoryName, dbo.vApplications.companyName, dbo.vApplications.reference, dbo.vApplications.current_desk, dbo.vApplications.user_id, 
                         dbo.vApplications.submitted, dbo.vApplications.PhaseId, dbo.vApplications.PhaseName, dbo.vApplications.FacilityName, dbo.vApplications.AddressId, dbo.vApplications.address_1, dbo.vApplications.city, 
                         dbo.vApplications.StateName, dbo.vApplications.CountryName, dbo.vApplications.FacilityId, dbo.vApplications.AllowPush, dbo.vJointAccountStaffs.Opscon, dbo.vJointAccountStaffs.DateAdded, 
                         dbo.vJointAccountStaffs.OperationsCompleted, dbo.vJointAccountStaffs.Assigned, dbo.vJointAccountStaffs.SignedOff
FROM            dbo.vApplications INNER JOIN
                         dbo.vJointAccountStaffs ON dbo.vApplications.id = dbo.vJointAccountStaffs.ApplicationId