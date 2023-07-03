CREATE TABLE [dbo].[OutOfOffice] (
    [OutID]           INT           IDENTITY (1, 1) NOT NULL,
    [StaffID]         INT           NOT NULL,
    [ReliverID]       INT           NOT NULL,
    [Comment]         VARCHAR (MAX) NOT NULL,
    [DateFrom]        DATETIME      NOT NULL,
    [DateTo]          DATETIME      NOT NULL,
    [CreatedAt]       DATETIME      NOT NULL,
    [UpdatedAt]       DATETIME      NULL,
    [DeletedAt]       DATETIME      NULL,
    [DeletedBy]       INT           NULL,
    [Status]          VARCHAR (10)  NULL,
    [DeletedStatus]   BIT           NULL,
    [Approved]        BIT           NULL,
    [ApprovedBy]      VARCHAR (100) NULL,
    [ApproverRole]    VARCHAR (50)  NULL,
    [ApprovedDate]    DATETIME      NULL,
    [ApproverComment] VARCHAR (100) NULL,
    CONSTRAINT [PK_OutOfOffice] PRIMARY KEY CLUSTERED ([OutID] ASC)
);



