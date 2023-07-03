CREATE TABLE [dbo].[FieldOfficeStates] (
    [FieldOfficeStatesId] INT      IDENTITY (1, 1) NOT NULL,
    [FieldOffice_id]       INT      NOT NULL,
    [StateId]             INT      NOT NULL,
    [CreatedAt]           DATETIME NOT NULL,
    [UpdatedAt]           DATETIME NULL,
    [DeleteStatus]        BIT      NOT NULL,
    [DeletedBy]           INT      NULL,
    [DeletedAt]           DATETIME NULL,
    CONSTRAINT [PK_FieldOfficeStates] PRIMARY KEY CLUSTERED ([FieldOfficeStatesId] ASC)
);

