CREATE TABLE [dbo].[PhaseRoutings] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [PhaseId]            INT           NOT NULL,
    [ProcessingRule_Id]  INT           NOT NULL,
    [ProcessingLocation] NVARCHAR (50) NOT NULL,
    [SortOrder]          INT           NULL,
    [Deleted]            BIT           NOT NULL,
    CONSTRAINT [PK_inspection_routing_services] PRIMARY KEY CLUSTERED ([id] ASC)
);

