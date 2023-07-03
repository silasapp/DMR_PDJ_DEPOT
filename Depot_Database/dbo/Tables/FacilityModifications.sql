CREATE TABLE [dbo].[FacilityModifications] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [FacilityId]    INT           NOT NULL,
    [PhaseId]       INT           NOT NULL,
    [ApplicationId] INT           NOT NULL,
    [Date]          DATETIME      NULL,
    [Type]          NVARCHAR (50) NULL,
    [PrevProduct]   VARCHAR (500)  NULL,
    CONSTRAINT [PK_FacilityModifications] PRIMARY KEY CLUSTERED ([Id] ASC)
);

