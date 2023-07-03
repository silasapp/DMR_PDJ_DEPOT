CREATE TABLE [dbo].[company_documents] (
    [id]               INT            IDENTITY (16446, 1) NOT NULL,
    [company_id]       INT            NOT NULL,
    [document_type_id] INT            NOT NULL,
    [name]             VARCHAR (250)  NOT NULL,
    [type]             VARCHAR (50)   NOT NULL,
    [source]           VARCHAR (200)  NOT NULL,
    [date_modified]    DATETIME2 (0)  NOT NULL,
    [date_added]       DATETIME2 (0)  NOT NULL,
    [Status]           BIT            NULL,
    [archived]         BIT            NOT NULL,
    [document_name]    NVARCHAR (250) NULL,
    [UniqueId]         NVARCHAR (15)  NULL,
    [Elps_Id]          INT            NULL,
    CONSTRAINT [PK_company_document_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

