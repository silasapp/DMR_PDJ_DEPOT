CREATE TABLE [dbo].[Document_Type_Applications] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [DocumentTypeId] INT           NOT NULL,
    [ApplicationId]  INT           NOT NULL,
    [UniqueId]       NVARCHAR (50) NULL,
    CONSTRAINT [PK_Document_Type_Applications] PRIMARY KEY CLUSTERED ([Id] ASC)
);

