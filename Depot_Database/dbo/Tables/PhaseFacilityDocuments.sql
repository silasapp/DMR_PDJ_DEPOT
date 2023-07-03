CREATE TABLE [dbo].[PhaseFacilityDocuments] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [document_type_id] INT NOT NULL,
    [PhaseId]          INT NOT NULL,
    CONSTRAINT [PK_PhaseFacilityDocuments] PRIMARY KEY CLUSTERED ([Id] ASC)
);

