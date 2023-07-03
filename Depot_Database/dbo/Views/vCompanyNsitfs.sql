CREATE VIEW [dbo].[vCompanyNsitfs]
AS
SELECT        dbo.company_nsitfs.id, dbo.company_nsitfs.no_people_covered, dbo.company_nsitfs.policy_no, dbo.company_nsitfs.company_id, dbo.company_nsitfs.date_issued, dbo.company_nsitfs.FileId, 
                         dbo.files.name AS FileName, dbo.files.source AS FileSource
FROM            dbo.company_nsitfs LEFT OUTER JOIN
                         dbo.files ON dbo.company_nsitfs.FileId = dbo.files.id