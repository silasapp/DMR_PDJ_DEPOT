CREATE TABLE [dbo].[application_documents] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [application_id]   INT           NOT NULL,
    [document_id]      INT           NOT NULL,
    [document_type_id] INT           NOT NULL,
    [UniqueId]         NVARCHAR (15) NULL,
    [Status]           BIT           NULL,
    CONSTRAINT [PK_application_document_application_id] PRIMARY KEY CLUSTERED ([Id] ASC)
);

