CREATE TABLE [dbo].[ApplicationTanks] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [TankName]      NVARCHAR (150) NOT NULL,
    [TankId]        INT            NOT NULL,
    [Capacity]      FLOAT (53)     NOT NULL,
    [FacilityId]    INT            NOT NULL,
    [CompanyId]     INT            NOT NULL,
    [ApplicationId] INT            NOT NULL,
    [Date]          DATETIME       NOT NULL,
    [ProductId]     INT            NOT NULL,
    [UpdatedAt]     DATETIME       NULL,
    CONSTRAINT [PK_ApplicationTanks] PRIMARY KEY CLUSTERED ([Id] ASC)
);

