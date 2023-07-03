CREATE TABLE [dbo].[AuditLogs] (
    [AuditLogId]    UNIQUEIDENTIFIER NOT NULL,
    [UserId]        NVARCHAR (50)    NULL,
    [EventDateUTC]  DATETIME         NOT NULL,
    [EventType]     NCHAR (1)        NULL,
    [TableName]     NVARCHAR (100)   NULL,
    [RecordId]      NVARCHAR (100)   NULL,
    [ColumnName]    NVARCHAR (100)   NULL,
    [OriginalValue] NVARCHAR (MAX)   NULL,
    [NewValue]      NVARCHAR (MAX)   NULL,
    [IP]            NVARCHAR (50)    NULL,
    CONSTRAINT [PK_AuditLogs_1] PRIMARY KEY CLUSTERED ([AuditLogId] ASC)
);



