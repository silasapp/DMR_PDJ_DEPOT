CREATE VIEW [dbo].[vFacilitiesWithAtleasOneApplications]
AS
SELECT        TOP (100) PERCENT facilities.id, facilities.name, dbo.Facilities.CompanyId, dbo.Facilities.Date, facilities.elps_id, addresses.city, States.name AS StateName, dbo.Facilities.ContactName, dbo.Facilities.IdentificationCode, 
                         dbo.Facilities.CategoryCode, dbo.Facilities.FirstOperationYear, dbo.addresses.address_1 AS StreetAddress
FROM            dbo.Facilities INNER JOIN
                         dbo.addresses ON addresses.id = facilities.AddressId INNER JOIN
                         dbo.States ON States.Id = addresses.StateId
WHERE        ((SELECT        COUNT(id) AS Expr1
                            FROM            dbo.applications
                            WHERE        (FacilityId = facilities.id) AND (status <> 'Payment Pending')) > 0)
ORDER BY dbo.Facilities.Name