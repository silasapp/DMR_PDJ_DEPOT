CREATE TABLE [dbo].[FieldValues] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [FieldId]       INT              NOT NULL,
    [Value]         NVARCHAR (MAX)   NOT NULL,
    [GroupId]       UNIQUEIDENTIFIER NULL,
    [ApplicationId] INT              NULL,
    [FormId]        INT              NULL,
    CONSTRAINT [PK_FieldValues] PRIMARY KEY CLUSTERED ([Id] ASC)
);

