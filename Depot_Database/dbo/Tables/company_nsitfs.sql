CREATE TABLE [dbo].[company_nsitfs] (
    [id]                INT           IDENTITY (2688, 1) NOT NULL,
    [no_people_covered] INT           NOT NULL,
    [policy_no]         VARCHAR (100) NOT NULL,
    [company_id]        INT           NOT NULL,
    [date_issued]       DATE          NOT NULL,
    [FileId]            INT           NULL,
    [Elps_Id]           INT           NULL,
    CONSTRAINT [PK_company_nsitf_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

