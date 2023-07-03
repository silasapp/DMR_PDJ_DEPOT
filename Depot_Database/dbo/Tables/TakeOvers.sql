CREATE TABLE [dbo].[TakeOvers] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [FacilityId]      INT            NOT NULL,
    [CompanyId]       INT            NOT NULL,
    [OldCompanyId]    INT            NOT NULL,
    [ApplicationId]   INT            NOT NULL,
    [Date]            DATETIME       NOT NULL,
    [OldCompanyDPRId] NVARCHAR (100) NULL,
    CONSTRAINT [PK_TakeOvers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

