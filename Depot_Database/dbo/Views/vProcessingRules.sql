CREATE VIEW [dbo].[vProcessingRules]
AS
SELECT        dbo.ProcessingRules.Id, dbo.ProcessingRules.DepartmentId, dbo.ProcessingRules.RoleId, dbo.Departments.Name AS DepartmentName, dbo.WorkRoles.Name AS RoleName
FROM            dbo.ProcessingRules INNER JOIN
                         dbo.Departments ON dbo.ProcessingRules.DepartmentId = dbo.Departments.Id INNER JOIN
                         dbo.WorkRoles ON dbo.ProcessingRules.RoleId = dbo.WorkRoles.Id