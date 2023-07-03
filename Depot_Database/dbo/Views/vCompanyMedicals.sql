CREATE VIEW [dbo].[vCompanyMedicals]
AS
SELECT        dbo.company_medicals.id, dbo.company_medicals.medical_organisation, dbo.company_medicals.address, dbo.company_medicals.phone, dbo.company_medicals.email, dbo.company_medicals.company_id, 
                         dbo.company_medicals.date_issued, dbo.company_medicals.FileId, dbo.files.name AS FileName, dbo.files.source AS FileSource
FROM            dbo.company_medicals LEFT OUTER JOIN
                         dbo.files ON dbo.company_medicals.FileId = dbo.files.id