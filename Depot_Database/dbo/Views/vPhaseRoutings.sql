CREATE VIEW [dbo].[vPhaseRoutings]
AS
SELECT        dbo.PhaseRoutings.id, dbo.PhaseRoutings.PhaseId, dbo.PhaseRoutings.ProcessingLocation, dbo.PhaseRoutings.SortOrder, dbo.PhaseRoutings.Deleted, dbo.vProcessingRules.DepartmentName, 
                         dbo.vProcessingRules.RoleName, dbo.PhaseRoutings.ProcessingRule_Id
FROM            dbo.PhaseRoutings INNER JOIN
                         dbo.vProcessingRules ON dbo.PhaseRoutings.ProcessingRule_Id = dbo.vProcessingRules.Id