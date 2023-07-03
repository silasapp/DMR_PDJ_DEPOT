CREATE TABLE [dbo].[company_technical_agreements] (
    [id]         INT           IDENTITY (241, 1) NOT NULL,
    [company_id] INT           NOT NULL,
    [name]       VARCHAR (200) NOT NULL,
    [FileId]     INT           NOT NULL,
    [Elps_Id]    INT           NULL,
    CONSTRAINT [PK_company_technical_agreement_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

