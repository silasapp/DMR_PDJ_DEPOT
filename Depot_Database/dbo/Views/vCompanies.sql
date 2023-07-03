


CREATE VIEW [dbo].[vCompanies]
AS
SELECT        dbo.companies.id, dbo.companies.registered_address_id, dbo.addresses.address_1, dbo.companies.Name, dbo.companies.User_Id,  dbo.companies.Business_Type, dbo.companies.Nationality, dbo.companies.Contact_FirstName,
					 dbo.companies.Contact_LastName, dbo.companies.Contact_Phone, dbo.companies.Year_Incorporated, dbo.companies.RC_Number,
					  dbo.companies.Tin_Number, 
					 dbo.addresses.city, dbo.states.Name as StateName, dbo.countries.name as CountryName
FROM            dbo.companies INNER JOIN
                         dbo.addresses ON dbo.companies.registered_address_id = dbo.addresses.id inner join 
						 dbo.states on dbo.states.id=dbo.addresses.stateId inner join
						 dbo.countries on dbo.countries.id=dbo.addresses.country_id