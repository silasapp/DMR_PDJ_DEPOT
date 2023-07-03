CREATE TABLE [dbo].[zones] (
    [id]         INT            IDENTITY (4033, 1) NOT NULL,
    [country_id] INT            NOT NULL,
    [name]       NVARCHAR (128) NOT NULL,
    [code]       NVARCHAR (32)  NOT NULL,
    [status]     SMALLINT       CONSTRAINT [DF__zones__status__51EF2864] DEFAULT ((1)) NOT NULL,
    [BranchId]   INT            NOT NULL,
    CONSTRAINT [PK_zone_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

