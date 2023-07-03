CREATE VIEW [dbo].[vRequiredFiles]
AS
SELECT        dbo.document_type_categories.Id, dbo.document_type_categories.category_id, dbo.document_type_categories.document_type_id, dbo.document_types.name AS DocumentTypeName, 
                         dbo.categories.name AS CategoryName
FROM            dbo.document_type_categories INNER JOIN
                         dbo.document_types ON dbo.document_type_categories.document_type_id = dbo.document_types.id INNER JOIN
                         dbo.categories ON dbo.document_type_categories.category_id = dbo.categories.id