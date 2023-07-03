CREATE TABLE [dbo].[company_medicals] (
    [id]                   INT           IDENTITY (2649, 1) NOT NULL,
    [medical_organisation] VARCHAR (250) NOT NULL,
    [address]              VARCHAR (250) NULL,
    [phone]                VARCHAR (20)  NULL,
    [email]                VARCHAR (200) NULL,
    [company_id]           INT           NOT NULL,
    [date_issued]          DATE          NOT NULL,
    [FileId]               INT           NULL,
    [Elps_Id]              INT           NULL,
    CONSTRAINT [PK_company_medical_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

