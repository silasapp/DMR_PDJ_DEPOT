

CREATE VIEW [dbo].[vCompAddressesU]
AS
SELECT        dbo.companies.id, dbo.companies.registered_address_id, dbo.addresses.address_1, dbo.addresses.address_2, dbo.addresses.city, dbo.addresses.postal_code, dbo.addresses.country_id, dbo.addresses.elps_id, 
                         dbo.addresses.StateId, dbo.addresses.LgaId, dbo.companies.Date
FROM            dbo.companies INNER JOIN
                         dbo.addresses ON dbo.companies.registered_address_id = dbo.addresses.id