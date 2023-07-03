CREATE VIEW [dbo].[vCompanyProffessionals]
AS
SELECT        dbo.company_proffessionals.id, dbo.company_proffessionals.proffessional_organisation, dbo.company_proffessionals.cert_no, dbo.company_proffessionals.company_id, 
                         dbo.company_proffessionals.date_issued, dbo.company_proffessionals.FileId, dbo.files.name AS FileName, dbo.files.source AS FileSource
FROM            dbo.company_proffessionals LEFT OUTER JOIN
                         dbo.files ON dbo.company_proffessionals.FileId = dbo.files.id