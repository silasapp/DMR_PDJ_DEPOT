--select * from dbo.companies where  date > '2018-10-09 15:00' and date < '2018-10-16 00:00'
--order by date desc

--update dbo.companies set
--elps_id=68482
-- where user_id='customerservice@africaterminals.com'


CREATE VIEW [dbo].[vCompanyAspUsers]
AS
SELECT        dbo.companies.id, dbo.companies.Date, dbo.companies.elps_id, dbo.AspNetUsers.Id AS AspUserId, dbo.AspNetUsers.Email, dbo.AspNetUsers.EmailConfirmed, dbo.AspNetUsers.PasswordHash, 
                         dbo.AspNetUsers.SecurityStamp, dbo.AspNetUsers.PhoneNumber, dbo.AspNetUsers.PhoneNumberConfirmed, dbo.AspNetUsers.TwoFactorEnabled, dbo.AspNetUsers.LockoutEndDateUtc, dbo.AspNetUsers.LockoutEnabled, 
                         dbo.AspNetUsers.AccessFailedCount, dbo.AspNetUsers.UserName
FROM            dbo.companies INNER JOIN
                         dbo.AspNetUsers ON dbo.companies.user_id = dbo.AspNetUsers.Email