CREATE VIEW [dbo].[vCompanyTechnicalAgreements]
AS
SELECT        dbo.company_technical_agreements.id, dbo.company_technical_agreements.company_id, dbo.company_technical_agreements.name, dbo.company_technical_agreements.FileId, dbo.files.name AS FileName, 
                         dbo.files.source AS FileSource
FROM            dbo.company_technical_agreements LEFT OUTER JOIN
                         dbo.files ON dbo.company_technical_agreements.FileId = dbo.files.id