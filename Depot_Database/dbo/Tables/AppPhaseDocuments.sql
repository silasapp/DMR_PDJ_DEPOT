CREATE TABLE [dbo].[AppPhaseDocuments] (
    [DocID]        INT      IDENTITY (1, 1) NOT NULL,
    [AppPhaseID]   INT      NOT NULL,
    [AppDocID]     INT      NOT NULL,
    [CreatedAt]    DATETIME NOT NULL,
    [UpdatedAt]    DATETIME NULL,
    [DeleteStatus] BIT      NOT NULL,
    [DeletedBy]    INT      NULL,
    [DeletedAt]    DATETIME NULL,
    CONSTRAINT [PK_AppStageDocuments] PRIMARY KEY CLUSTERED ([DocID] ASC)
);

