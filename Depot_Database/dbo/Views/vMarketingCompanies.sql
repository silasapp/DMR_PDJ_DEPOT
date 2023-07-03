CREATE VIEW [dbo].[vMarketingCompanies]
AS
SELECT        dbo.MarketingCompanies.Id, dbo.MarketingCompanies.SponsorId, dbo.MarketingCompanies.CompanyId, dbo.MarketingCompanies.FacilityId, dbo.MarketingCompanies.IsApproved, 
                         dbo.MarketingCompanies.ApprovedBy, dbo.MarketingCompanies.Date, dbo.vFacilities.Name AS FacilityName, dbo.vFacilities.CompanyName, dbo.MarketingCompanies.Reason
FROM            dbo.MarketingCompanies INNER JOIN
                         dbo.vFacilities ON dbo.MarketingCompanies.FacilityId = dbo.vFacilities.Id