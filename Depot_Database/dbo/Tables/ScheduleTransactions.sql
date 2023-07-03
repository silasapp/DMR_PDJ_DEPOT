CREATE TABLE [dbo].[ScheduleTransactions] (
    [ID]               BIGINT        IDENTITY (1, 1) NOT NULL,
    [EmployeeID]       BIGINT        NOT NULL,
    [ScheduleID]       BIGINT        NOT NULL,
    [ContributionType] NVARCHAR (50) NOT NULL,
    [TotalAmount]      MONEY         NULL,
    [EmployerAmount]   MONEY         NULL,
    [EmployeeAmount]   MONEY         NULL,
    [Date]             DATETIME      NOT NULL,
    CONSTRAINT [PK_ScheduleTransactions] PRIMARY KEY CLUSTERED ([ID] ASC)
);

