CREATE TABLE [dbo].[document_type_categories] (
    [document_type_id] INT NOT NULL,
    [category_id]      INT NOT NULL,
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_document_type_category] PRIMARY KEY CLUSTERED ([Id] ASC)
);

