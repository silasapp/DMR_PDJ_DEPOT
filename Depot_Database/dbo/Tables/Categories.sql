CREATE TABLE [dbo].[Categories] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [name]          NVARCHAR (50) NOT NULL,
    [FriendlyName]  NVARCHAR (50) NULL,
    [Price]         INT           NULL,
    [ServiceCharge] INT           NULL,
    [CreatedAt]     DATETIME      NULL,
    [CreatedBy]     INT           NULL,
    [UpdatedAt]     DATETIME      NULL,
    [UpdatedBy]     INT           NULL,
    [DeleteStatus]  BIT           NULL,
    [DeletedBy]     INT           NULL,
    [DeletedAt]     DATETIME      NULL,
    CONSTRAINT [PK_category_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

