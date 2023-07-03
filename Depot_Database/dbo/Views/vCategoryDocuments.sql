CREATE VIEW [dbo].[vCategoryDocuments]
AS
SELECT        dbo.document_types.id, dbo.document_types.name, dbo.document_type_categories.category_id
FROM            dbo.document_types INNER JOIN
                         dbo.document_type_categories ON dbo.document_types.id = dbo.document_type_categories.document_type_id