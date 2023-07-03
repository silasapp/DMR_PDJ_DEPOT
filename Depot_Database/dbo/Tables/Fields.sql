CREATE TABLE [dbo].[Fields] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [FormId]               INT            NOT NULL,
    [Label]                NVARCHAR (50)  NOT NULL,
    [IsRequired]           BIT            NOT NULL,
    [Validation]           NVARCHAR (150) NULL,
    [DataType]             NVARCHAR (50)  NOT NULL,
    [CreatedByUserId]      NVARCHAR (150) NULL,
    [CreatedOnDate]        DATETIME       NULL,
    [LastModifiedByUserId] NVARCHAR (150) NULL,
    [lastModifiedOnDate]   DATETIME       NULL,
    [OptionValue]          NVARCHAR (500) NULL,
    [DisplayLabel]         NVARCHAR (500) NULL,
    [Description]          NVARCHAR (500) NULL,
    [SortOrder]            INT            NULL,
    [hiddenValue]          VARCHAR (50)   NULL,
    CONSTRAINT [PK_Fields] PRIMARY KEY CLUSTERED ([Id] ASC)
);

