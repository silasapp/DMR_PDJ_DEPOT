CREATE VIEW [dbo].[vApplicationProcessingRules]
AS
SELECT        dbo.application_Processings.Id, dbo.application_Processings.SortOrder, dbo.application_Processings.processor, dbo.application_Processings.DateProcessed, dbo.application_Processings.ProcessingRule_Id, 
                         dbo.application_Processings.Processed, dbo.application_Processings.ApplicationId, dbo.application_Processings.Assigned, dbo.application_Processings.ProcessingLocation, 
                         dbo.ProcessingRules.DepartmentId, dbo.ProcessingRules.RoleId
FROM            dbo.application_Processings INNER JOIN
                         dbo.ProcessingRules ON dbo.application_Processings.ProcessingRule_Id = dbo.ProcessingRules.Id