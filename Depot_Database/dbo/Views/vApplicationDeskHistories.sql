CREATE VIEW [dbo].[vApplicationDeskHistories]
AS
SELECT        dbo.application_desk_histories.id, dbo.application_desk_histories.application_id, dbo.application_desk_histories.UserName, dbo.application_desk_histories.comment, dbo.application_desk_histories.date, 
                         dbo.application_desk_histories.status, dbo.applications.type, dbo.applications.year, dbo.applications.date_added AS ApplicationDate, dbo.categories.name AS CategoryName, 
                         dbo.companies.name AS CompanyName, dbo.applications.reference
FROM            dbo.application_desk_histories INNER JOIN
                         dbo.applications ON dbo.application_desk_histories.application_id = dbo.applications.id INNER JOIN
                         dbo.categories ON dbo.applications.category_id = dbo.categories.id INNER JOIN
                         dbo.companies ON dbo.applications.company_id = dbo.companies.id