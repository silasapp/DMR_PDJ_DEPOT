CREATE VIEW [dbo].[vUserBranches]
AS
SELECT        dbo.UserBranches.UserEmail, dbo.UserBranches.BranchId, dbo.UserBranches.RoleId, dbo.UserBranches.DepartmentId, dbo.UserBranches.Id,
                             (SELECT        COUNT(Id) AS Expr1
                               FROM            dbo.application_Processings
                               WHERE        (processor = dbo.UserBranches.Id) AND (Processed = 0)) AS DeskCount, dbo.Departments.Name AS DepartmentName, dbo.branches.name AS BranchName, dbo.branches.location, dbo.WorkRoles.Name AS Role, 
                         dbo.UserBranches.Active, dbo.AspNetUsers.Id AS UserId
FROM            dbo.UserBranches LEFT OUTER JOIN
                         dbo.branches ON dbo.UserBranches.BranchId = dbo.branches.id INNER JOIN
                         dbo.Departments ON dbo.UserBranches.DepartmentId = dbo.Departments.Id INNER JOIN
                         dbo.WorkRoles ON dbo.UserBranches.RoleId = dbo.WorkRoles.Id INNER JOIN
                         dbo.AspNetUsers ON dbo.UserBranches.UserEmail = dbo.AspNetUsers.Email