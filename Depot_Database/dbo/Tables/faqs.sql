CREATE TABLE [dbo].[faqs] (
    [id]   INT          IDENTITY (64, 1) NOT NULL,
    [name] VARCHAR (70) NOT NULL,
    CONSTRAINT [PK_faq_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

