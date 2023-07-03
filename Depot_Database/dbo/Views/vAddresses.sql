CREATE VIEW [dbo].[vAddresses]
AS
SELECT        dbo.addresses.id, dbo.addresses.address_1, dbo.addresses.address_2, dbo.addresses.city, dbo.addresses.country_id, dbo.addresses.StateId, dbo.countries.name AS CountryName, 
                         dbo.States.Name AS StateName, dbo.addresses.postal_code, dbo.addresses.elps_id, dbo.Lgas.Name AS Lga
FROM            dbo.addresses INNER JOIN
                         dbo.countries ON dbo.addresses.country_id = dbo.countries.id INNER JOIN
                         dbo.States ON dbo.addresses.StateId = dbo.States.Id LEFT OUTER JOIN
                         dbo.Lgas ON dbo.addresses.LgaId = dbo.Lgas.Id