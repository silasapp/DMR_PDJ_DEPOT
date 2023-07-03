CREATE TABLE [dbo].[GeoPoliticalStates] (
    [GeoStateId]    INT      IDENTITY (1, 1) NOT NULL,
    [GeoId]         INT      NOT NULL,
    [StateId]       INT      NOT NULL,
    [CreatedAt]     DATETIME NOT NULL,
    [UpdatedAt]     DATETIME NULL,
    [DeletedStatus] BIT      NOT NULL,
    [DeletedBy]     INT      NULL,
    [DeletedAt]     DATETIME NULL,
    PRIMARY KEY CLUSTERED ([GeoStateId] ASC)
);

