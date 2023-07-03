CREATE TABLE [dbo].[Pumps] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50) NOT NULL,
    [TankId]       INT           NOT NULL,
    [Manufacturer] NVARCHAR (50) NOT NULL,
    [Model]        NVARCHAR (50) NULL,
    [Version]      NVARCHAR (50) NULL,
    [FacilityId]   INT           NOT NULL,
    CONSTRAINT [PK_Pumps] PRIMARY KEY CLUSTERED ([Id] ASC)
);

