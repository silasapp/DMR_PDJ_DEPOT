CREATE VIEW [dbo].[vCategoryRoutings]
AS
SELECT        dbo.vProcessingRules.RoleName, dbo.vProcessingRules.DepartmentName, dbo.CategoryRoutings.Id, dbo.CategoryRoutings.category_id, dbo.CategoryRoutings.ProcessingLocation, 
                         dbo.CategoryRoutings.SortOrder, dbo.CategoryRoutings.Deleted, dbo.CategoryRoutings.ProcessingRule_id
FROM            dbo.CategoryRoutings INNER JOIN
                         dbo.vProcessingRules ON dbo.CategoryRoutings.ProcessingRule_id = dbo.vProcessingRules.Id