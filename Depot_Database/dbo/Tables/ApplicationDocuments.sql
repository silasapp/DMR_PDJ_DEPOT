CREATE TABLE [dbo].[ApplicationDocuments] (
    [AppDocID]      INT           IDENTITY (1, 1) NOT NULL,
    [ElpsDocTypeID] INT           NOT NULL,
    [DocName]       VARCHAR (200) NOT NULL,
    [CreatedAt]     DATETIME      NOT NULL,
    [UpdatedAt]     DATETIME      NULL,
    [DeleteStatus]  BIT           NOT NULL,
    [DeletedBy]     INT           NULL,
    [DeletedAt]     DATETIME      NULL,
    [docType]       VARCHAR (8)   NOT NULL,
    [PhaseId]       INT           NULL,
    CONSTRAINT [PK_ApplicationDocuments_UT] PRIMARY KEY CLUSTERED ([AppDocID] ASC)
);



