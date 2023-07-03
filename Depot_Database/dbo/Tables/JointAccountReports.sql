CREATE TABLE [dbo].[JointAccountReports] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [JointAccountId] INT            NOT NULL,
    [Report]         NVARCHAR (MAX) NULL,
    [Reportby]       NVARCHAR (250) NULL,
    [ReportDate]     DATETIME       NULL,
    [ApplicationId]  INT            NULL,
    CONSTRAINT [PK_JointAccountReport] PRIMARY KEY CLUSTERED ([Id] ASC)
);

