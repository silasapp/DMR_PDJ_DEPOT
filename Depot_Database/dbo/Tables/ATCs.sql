CREATE TABLE [dbo].[ATCs] (
    [Id]            INT IDENTITY (1, 1) NOT NULL,
    [CompanyId]     INT NOT NULL,
    [SuitabilityId] INT NOT NULL,
    [ApplicationId] INT NOT NULL,
    [FacilityId]    INT NULL,
    CONSTRAINT [PK_ApprovaToConstructs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

