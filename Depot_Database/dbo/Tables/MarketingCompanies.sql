CREATE TABLE [dbo].[MarketingCompanies] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [SponsorId]  INT            NOT NULL,
    [CompanyId]  INT            NOT NULL,
    [FacilityId] INT            NOT NULL,
    [IsApproved] BIT            NOT NULL,
    [ApprovedBy] NVARCHAR (250) NULL,
    [Date]       DATETIME       NOT NULL,
    [Reason]     NVARCHAR (500) NULL,
    CONSTRAINT [PK_MarketingCompanies] PRIMARY KEY CLUSTERED ([Id] ASC)
);

