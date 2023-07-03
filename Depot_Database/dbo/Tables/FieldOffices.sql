CREATE TABLE [dbo].[FieldOffices] (
    [FieldOffice_id]      INT           IDENTITY (1, 1) NOT NULL,
    [OfficeName]          VARCHAR (50)  NOT NULL,
    [CreatedAt]           DATETIME      NOT NULL,
    [UpdatedAt]           DATETIME      NULL,
    [DeleteStatus]        BIT           NOT NULL,
    [DeletedBy]           INT           NULL,
    [DeletedAt]           DATETIME      NULL,
    [ZoneFieldOffice_id] INT           NULL,
    [ELPSID]              INT           NULL,
    [OfficeAddress]       VARCHAR (200) NULL,
    [FieldType]           VARCHAR (50)  NULL,
    [StateName]           VARCHAR (30)  NULL,
    CONSTRAINT [PK_FieldOffice] PRIMARY KEY CLUSTERED ([FieldOffice_id] ASC)
);

