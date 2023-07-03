CREATE TABLE [dbo].[files] (
    [id]         INT            IDENTITY (16656, 1) NOT NULL,
    [model_id]   INT            NOT NULL,
    [sort_order] INT            NOT NULL,
    [size]       NVARCHAR (10)  NOT NULL,
    [mime]       NVARCHAR (100) NOT NULL,
    [name]       NVARCHAR (50)  NOT NULL,
    [source]     NVARCHAR (255) NOT NULL,
    [model_name] NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_file_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

