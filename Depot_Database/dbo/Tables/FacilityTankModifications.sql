CREATE TABLE [dbo].[FacilityTankModifications] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [TankId]        INT           NOT NULL,
    [ApplicationId] INT           NOT NULL,
    [ModifyType]    NVARCHAR (50) NULL,
    [Type]          NVARCHAR (50) NULL,
    [PrevProduct]   VARCHAR (50)  NULL,
    CONSTRAINT [PK_FacilityTankModifications] PRIMARY KEY CLUSTERED ([Id] ASC)
);

