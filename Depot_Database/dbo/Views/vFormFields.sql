CREATE VIEW [dbo].[vFormFields]
AS
SELECT        dbo.Forms.Id AS FormId, dbo.Fields.Id, dbo.Fields.Label, dbo.Fields.IsRequired, dbo.Fields.Validation, dbo.Fields.DataType, dbo.Fields.OptionValue, dbo.Fields.DisplayLabel, dbo.Fields.Description, 
                         dbo.Fields.SortOrder, dbo.Forms.TableName AS FriendlyName, dbo.Forms.Title
FROM            dbo.Forms INNER JOIN
                         dbo.Fields ON dbo.Forms.Id = dbo.Fields.FormId