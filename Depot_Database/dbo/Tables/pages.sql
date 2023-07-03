CREATE TABLE [dbo].[pages] (
    [id]               INT           IDENTITY (8, 1) NOT NULL,
    [name]             VARCHAR (50)  NOT NULL,
    [title]            VARCHAR (100) NOT NULL,
    [meta_keyword]     VARCHAR (100) NOT NULL,
    [meta_description] VARCHAR (MAX) NOT NULL,
    [content]          VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_page_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

