CREATE VIEW [dbo].[vFacilities]
AS
SELECT        dbo.Facilities.Id, dbo.Facilities.Name, dbo.Facilities.CompanyId, dbo.Facilities.CategoryId, dbo.Facilities.AddressId, dbo.Facilities.CoordinateId, dbo.Facilities.Date, dbo.Facilities.NoofDriveIn, dbo.Facilities.NoOfDriveOut, 
                         dbo.companies.name AS CompanyName, dbo.companies.contact_phone, dbo.companies.user_id, dbo.vAddresses.address_1, dbo.vAddresses.city, dbo.vAddresses.CountryName, dbo.vAddresses.StateName,
                             (SELECT        COUNT(Id) AS Expr1
                               FROM            dbo.Pumps
                               WHERE        (FacilityId = dbo.Facilities.Id)) AS NoOfPumps,
                             (SELECT        COUNT(Id) AS Expr2
                               FROM            dbo.Tanks
                               WHERE        (FacilityId = dbo.Facilities.Id)) AS NoOfTanks,
                             (SELECT        SUM(CAST(MaxCapacity AS float)) AS exp3
                               FROM            dbo.Tanks AS Tanks_1
                               WHERE        (FacilityId = dbo.Facilities.Id)) AS MaxCapacity, dbo.Facilities.Elps_Id, dbo.vAddresses.Lga, dbo.Facilities.ContactName, dbo.Facilities.ContactNumber, dbo.vAddresses.StateId, 
                         dbo.Facilities.IdentificationCode
FROM            dbo.Facilities INNER JOIN
                         dbo.companies ON dbo.Facilities.CompanyId = dbo.companies.id INNER JOIN
                         dbo.vAddresses ON dbo.Facilities.AddressId = dbo.vAddresses.id