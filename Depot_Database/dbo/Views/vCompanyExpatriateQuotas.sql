CREATE VIEW [dbo].[vCompanyExpatriateQuotas]
AS
SELECT        dbo.company_expatriate_quotas.id, dbo.company_expatriate_quotas.company_id, dbo.company_expatriate_quotas.name, dbo.company_expatriate_quotas.FileId, dbo.files.name AS FileName, 
                         dbo.files.source AS FileSource
FROM            dbo.company_expatriate_quotas LEFT OUTER JOIN
                         dbo.files ON dbo.company_expatriate_quotas.FileId = dbo.files.id