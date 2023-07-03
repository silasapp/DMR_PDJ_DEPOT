

CREATE VIEW [dbo].[vApplicationTanks]
AS
SELECT        dbo.ApplicationTanks.Id, dbo.ApplicationTanks.TankName, dbo.ApplicationTanks.TankId, dbo.ApplicationTanks.Capacity, dbo.ApplicationTanks.FacilityId, dbo.ApplicationTanks.CompanyId, dbo.ApplicationTanks.ApplicationId, 
                         dbo.ApplicationTanks.Date, dbo.ApplicationTanks.ProductId, dbo.vTanks.ProductName, dbo.vTanks.FriendlyName, dbo.vTanks.Diameter, dbo.vTanks.Height
FROM            dbo.ApplicationTanks INNER JOIN
                         dbo.vTanks ON dbo.ApplicationTanks.TankId = dbo.vTanks.Id