CREATE TABLE [dbo].[document_types] (
    [id]   INT           IDENTITY (12, 1) NOT NULL,
    [name] VARCHAR (250) NOT NULL,
    CONSTRAINT [PK_document_type_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

