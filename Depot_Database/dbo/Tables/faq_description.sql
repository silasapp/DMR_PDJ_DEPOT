CREATE TABLE [dbo].[faq_description] (
    [id]          INT           IDENTITY (64, 1) NOT NULL,
    [faq_id]      INT           NULL,
    [locale_code] VARCHAR (2)   NOT NULL,
    [title]       CHAR (70)     NOT NULL,
    [description] VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_faq_description_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

