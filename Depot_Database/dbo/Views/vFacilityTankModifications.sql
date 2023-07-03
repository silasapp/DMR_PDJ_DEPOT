CREATE VIEW [dbo].[vFacilityTankModifications]
AS
SELECT        dbo.FacilityTankModifications.Id, dbo.FacilityTankModifications.TankId, dbo.FacilityTankModifications.ApplicationId, dbo.FacilityTankModifications.ModifyType, dbo.vTanks.FacilityId, dbo.vTanks.CompanyId, dbo.vTanks.Name, 
                         dbo.vTanks.ProductName, dbo.vTanks.Diameter, dbo.vTanks.Height, dbo.vTanks.MaxCapacity, dbo.FacilityTankModifications.PrevProduct
FROM            dbo.FacilityTankModifications INNER JOIN
                         dbo.vTanks ON dbo.FacilityTankModifications.TankId = dbo.vTanks.Id