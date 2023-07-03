CREATE VIEW [dbo].[vCompanyDirectors]
AS
SELECT        dbo.company_directors.id, dbo.company_directors.company_id, dbo.company_directors.firstname, dbo.company_directors.lastname, dbo.company_directors.address_id, dbo.company_directors.telephone, 
                         dbo.vAddresses.address_1, dbo.vAddresses.address_2, dbo.vAddresses.city, dbo.vAddresses.country_id, dbo.vAddresses.StateId, dbo.vAddresses.CountryName, dbo.vAddresses.StateName, 
                         dbo.vAddresses.postal_code AS PostalCode, dbo.company_directors.nationality, dbo.company_directors.elps_id
FROM            dbo.company_directors INNER JOIN
                         dbo.vAddresses ON dbo.company_directors.address_id = dbo.vAddresses.id