CREATE TABLE [dbo].[ApplicationRequest] (
    [RequestID]          INT          IDENTITY (1, 1) NOT NULL,
    [CompanyID]          INT          NOT NULL,
    [DurationID]         INT          NOT NULL,
    [DeskID]             INT          NULL,
    [CurrentStaffDeskID] INT          NULL,
    [RequestRefNo]       VARCHAR (50) NOT NULL,
    [GeneratedNumber]    VARCHAR (50) NOT NULL,
    [RequestSequence]    INT          NULL,
    [EmailSent]          BIT          NOT NULL,
    [hasAcknowledge]     BIT          NOT NULL,
    [AcknowledgeAt]      DATETIME     NULL,
    [Status]             VARCHAR (25) NULL,
    [isSubmitted]        BIT          NULL,
    [CreatedAt]          DATETIME     NOT NULL,
    [UpdatedAt]          DATETIME     NULL,
    [DeletedBy]          INT          NULL,
    [DeletedStatus]      BIT          NULL,
    [DeletedAt]          DATETIME     NULL,
    [DateApplied]        DATETIME     NULL,
    CONSTRAINT [PK__RequestP__33A8519A967F4C5A] PRIMARY KEY CLUSTERED ([RequestID] ASC)
);

