CREATE TABLE [dbo].[company_proffessionals] (
    [id]                         INT           IDENTITY (2936, 1) NOT NULL,
    [proffessional_organisation] VARCHAR (250) NOT NULL,
    [cert_no]                    VARCHAR (250) NULL,
    [company_id]                 INT           NOT NULL,
    [date_issued]                DATE          NULL,
    [FileId]                     INT           NULL,
    [Elps_Id]                    INT           NULL,
    CONSTRAINT [PK_company_proffessional_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

