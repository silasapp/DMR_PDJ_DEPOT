CREATE TABLE [dbo].[GeoPoliticalZone] (
    [GeoId]         INT          IDENTITY (1, 1) NOT NULL,
    [GeoName]       VARCHAR (30) NOT NULL,
    [CreatedAt]     DATETIME     NOT NULL,
    [UpdatedAt]     DATETIME     NULL,
    [DeletedStatus] BIT          NOT NULL,
    [DeletedBy]     INT          NULL,
    [DeletedAt]     DATETIME     NULL,
    PRIMARY KEY CLUSTERED ([GeoId] ASC)
);

