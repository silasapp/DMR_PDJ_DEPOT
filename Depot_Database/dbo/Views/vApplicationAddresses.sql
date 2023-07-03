
CREATE VIEW [dbo].[vApplicationAddresses]
AS
SELECT        dbo.applications.type, dbo.applications.id,  dbo.companies.id AS CompanyId, dbo.addresses.StateId, dbo.States.Name AS State, dbo.companies.name AS CompanyName
FROM            dbo.applications INNER JOIN
                         dbo.companies ON dbo.applications.company_id = dbo.companies.id INNER JOIN
                         dbo.addresses ON dbo.companies.operational_address_id = dbo.addresses.id INNER JOIN
                         dbo.States ON dbo.addresses.StateId = dbo.States.Id