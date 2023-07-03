CREATE VIEW [dbo].[vApplicationDocuments]
AS
SELECT        dbo.application_documents.Id, dbo.application_documents.application_id, dbo.application_documents.document_id, dbo.application_documents.document_type_id, 
                         dbo.document_types.name AS documentTypeName, dbo.company_documents.source, dbo.company_documents.name AS FileName, dbo.company_documents.Status, dbo.company_documents.document_name, 
                         dbo.application_documents.UniqueId, dbo.applications.company_id
FROM            dbo.application_documents INNER JOIN
                         dbo.document_types ON dbo.application_documents.document_type_id = dbo.document_types.id INNER JOIN
                         dbo.applications ON dbo.application_documents.application_id = dbo.applications.id LEFT OUTER JOIN
                         dbo.company_documents ON dbo.application_documents.document_id = dbo.company_documents.id