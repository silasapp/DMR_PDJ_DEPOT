CREATE TABLE [dbo].[Schedules] (
    [ID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [EmployerID]  BIGINT         NOT NULL,
    [AccID]       INT            NOT NULL,
    [Year]        INT            NULL,
    [Month]       NVARCHAR (50)  NULL,
    [TotalAmount] MONEY          NULL,
    [Date]        DATETIME       NOT NULL,
    [fileUrl]     NVARCHAR (250) NULL,
    CONSTRAINT [PK_Schedules] PRIMARY KEY CLUSTERED ([ID] ASC)
);

