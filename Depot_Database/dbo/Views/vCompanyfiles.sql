CREATE VIEW [dbo].[vCompanyfiles]
AS
SELECT        dbo.company_documents.id, dbo.company_documents.company_id AS companyId, dbo.company_documents.document_type_id, dbo.company_documents.source, dbo.company_documents.date_modified, 
                         dbo.company_documents.date_added, dbo.document_types.name AS DocumentTypeName, dbo.company_documents.name AS FileName, dbo.company_documents.Status, dbo.company_documents.archived, 
                         dbo.company_documents.document_name, dbo.company_documents.UniqueId, dbo.company_documents.Elps_Id
FROM            dbo.company_documents LEFT OUTER JOIN
                         dbo.document_types ON dbo.company_documents.document_type_id = dbo.document_types.id