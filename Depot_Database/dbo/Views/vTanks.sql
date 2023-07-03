CREATE VIEW [dbo].[vTanks]
AS
SELECT        dbo.Tanks.Id, dbo.Tanks.FacilityId, dbo.Tanks.CompanyId, dbo.Tanks.Name, dbo.Tanks.HasATG, dbo.Tanks.MaxCapacity, dbo.Tanks.ProductId, dbo.Tanks.Position, dbo.Products.Name AS ProductName, 
                         dbo.Products.FriendlyName, dbo.Tanks.Height, dbo.Tanks.Diameter, dbo.Tanks.Decommissioned
FROM            dbo.Tanks INNER JOIN
                         dbo.Products ON dbo.Tanks.ProductId = dbo.Products.Id