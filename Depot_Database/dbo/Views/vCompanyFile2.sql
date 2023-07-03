CREATE VIEW [dbo].[vCompanyFile2]
AS
SELECT        dbo.files.id, dbo.companies.id AS CompanyId, dbo.files.name AS FileName, dbo.files.source, dbo.application_documents.document_id, dbo.application_documents.document_type_id, 
                         dbo.document_types.name AS DocumentTypeName
FROM            dbo.files INNER JOIN
                         dbo.companies ON dbo.companies.id = dbo.files.model_id INNER JOIN
                         dbo.application_documents ON dbo.files.id = dbo.application_documents.document_id INNER JOIN
                         dbo.document_types ON dbo.application_documents.document_type_id = dbo.document_types.id