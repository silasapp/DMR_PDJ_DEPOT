CREATE TABLE [dbo].[CategoryRoutings] (
    [Id]                 INT           IDENTITY (1, 1) NOT NULL,
    [ProcessingRule_id]  INT           NOT NULL,
    [category_id]        INT           NOT NULL,
    [ProcessingLocation] NVARCHAR (50) NOT NULL,
    [SortOrder]          INT           NOT NULL,
    [Deleted]            BIT           NOT NULL,
    CONSTRAINT [PK_inspection_routing_categories] PRIMARY KEY CLUSTERED ([Id] ASC)
);

