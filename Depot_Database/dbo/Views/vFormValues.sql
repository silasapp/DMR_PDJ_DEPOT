CREATE VIEW [dbo].[vFormValues]
AS
SELECT        dbo.Fields.Label, dbo.FieldValues.Value, dbo.Fields.DataType, dbo.Fields.Id AS FieldId, dbo.Fields.FormId, dbo.FieldValues.Id, dbo.FieldValues.GroupId, dbo.Forms.Title, dbo.Forms.FriendlyName
FROM            dbo.Fields INNER JOIN
                         dbo.FieldValues ON dbo.Fields.Id = dbo.FieldValues.FieldId INNER JOIN
                         dbo.Forms ON dbo.Fields.FormId = dbo.Forms.Id