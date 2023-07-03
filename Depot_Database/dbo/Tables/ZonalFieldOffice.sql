CREATE TABLE [dbo].[ZonalFieldOffice] (
    [ZoneFieldOffice_id] INT      IDENTITY (1, 1) NOT NULL,
    [Zone_id]            INT      NOT NULL,
    [FieldOffice_id]     INT      NOT NULL,
    [CreatedAt]          DATETIME NOT NULL,
    [UpdatedAt]          DATETIME NULL,
    [DeleteStatus]       BIT      NOT NULL,
    [DeletedBy]          INT      NULL,
    [DeletedAt]          DATETIME NULL,
    CONSTRAINT [PK_ZonalFieldOffice] PRIMARY KEY CLUSTERED ([ZoneFieldOffice_id] ASC)
);

