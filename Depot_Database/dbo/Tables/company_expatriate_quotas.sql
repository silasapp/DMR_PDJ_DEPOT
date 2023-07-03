CREATE TABLE [dbo].[company_expatriate_quotas] (
    [id]         INT           IDENTITY (23, 1) NOT NULL,
    [company_id] INT           NOT NULL,
    [name]       VARCHAR (200) NOT NULL,
    [FileId]     INT           NULL,
    [Elps_Id]    INT           NULL,
    CONSTRAINT [PK_company_expatriate_quota_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

