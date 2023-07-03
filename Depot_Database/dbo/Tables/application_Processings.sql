CREATE TABLE [dbo].[application_Processings] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [SortOrder]          INT           NOT NULL,
    [DateProcessed]      DATETIME      NULL,
    [processor]          INT           NULL,
    [StaffEmail]         NVARCHAR (50) NULL,
    [ProcessingRule_Id]  INT           NULL,
    [Processed]          BIT           NULL,
    [ApplicationId]      INT           NOT NULL,
    [Assigned]           BIT           NOT NULL,
    [ProcessingLocation] NVARCHAR (10) NOT NULL,
    [Holding]            BIT           NULL,
    [Date_Assigned]      DATETIME      NULL,
    [AutoPushed]         BIT           NOT NULL,
    [AllowPush]          BIT           NULL,
    CONSTRAINT [PK_application_Processing] PRIMARY KEY CLUSTERED ([Id] ASC)
);

