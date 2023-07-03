CREATE VIEW [dbo].[vApplication_Processings]
AS
SELECT        dbo.application_Processings.Id, dbo.UserBranches.UserEmail, dbo.UserBranches.BranchId, dbo.application_Processings.ProcessingLocation, dbo.application_Processings.Assigned, 
                         dbo.application_Processings.ApplicationId, dbo.application_Processings.Processed, dbo.application_Processings.SortOrder, dbo.application_Processings.DateProcessed, 
                         dbo.application_Processings.ProcessingRule_Id, dbo.application_Processings.processor, dbo.vApplications.company_id, dbo.vApplications.type, dbo.vApplications.date_added, dbo.vApplications.CategoryName, 
                         dbo.vApplications.companyName, dbo.vApplications.year, dbo.application_Processings.Holding, dbo.vApplications.FacilityName, dbo.vApplications.FacilityId, dbo.vApplications.PhaseName, 
                         dbo.vApplications.AllowPush, dbo.vApplications.reference, dbo.WorkRoles.Name AS Role, dbo.vApplications.PhaseId
FROM            dbo.application_Processings INNER JOIN
                         dbo.vApplications ON dbo.application_Processings.ApplicationId = dbo.vApplications.id INNER JOIN
                         dbo.UserBranches ON dbo.application_Processings.processor = dbo.UserBranches.Id INNER JOIN
                         dbo.WorkRoles ON dbo.UserBranches.RoleId = dbo.WorkRoles.Id