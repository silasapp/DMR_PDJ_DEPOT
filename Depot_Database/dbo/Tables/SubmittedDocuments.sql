CREATE TABLE [dbo].[SubmittedDocuments] (
    [SubDocID]      INT           IDENTITY (1, 1) NOT NULL,
    [RequestID]     INT           NULL,
    [AppDocID]      INT           NOT NULL,
    [AppID]         INT           NOT NULL,
    [CompElpsDocID] INT           NULL,
    [CreatedAt]     DATETIME      NOT NULL,
    [UpdatedAt]     DATETIME      NULL,
    [DeletedStatus] BIT           NOT NULL,
    [DeletedBy]     INT           NULL,
    [DeletedAt]     DATETIME      NULL,
    [DocSource]     VARCHAR (MAX) NULL,
    [DocumentName]  VARCHAR (300) NULL,
    [IsAdditional]  BIT           NULL,
    CONSTRAINT [PK__Submitte__ADE7CAAC122BF9FC] PRIMARY KEY CLUSTERED ([SubDocID] ASC)
);

