CREATE VIEW [dbo].[vZones]
AS
SELECT        dbo.zones.id, dbo.zones.country_id, dbo.zones.name, dbo.zones.code, dbo.zones.status, dbo.countries.name AS CountryName, dbo.zones.BranchId, dbo.branches.name AS BranchName
FROM            dbo.zones LEFT OUTER JOIN
                         dbo.countries ON dbo.zones.country_id = dbo.countries.id INNER JOIN
                         dbo.branches ON dbo.zones.BranchId = dbo.branches.id