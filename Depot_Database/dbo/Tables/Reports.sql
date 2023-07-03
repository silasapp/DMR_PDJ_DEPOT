CREATE TABLE [dbo].[Reports] (
    [ReportId]       INT            IDENTITY (1, 1) NOT NULL,
    [AppId]          INT            NULL,
    [StaffId]        INT            NULL,
    [CreatedAt]      DATETIME       NULL,
    [StaffEmail]     VARCHAR (50)   NULL,
    [Title]          VARCHAR (100)  NULL,
    [Comment]        VARCHAR (500)  NULL,
    [UpdatedAt]      DATETIME       NULL,
    [DocSource]      VARCHAR (1000) NULL,
    [DeletedStatus]  BIT            NULL,
    [DeletedBy]      INT            NULL,
    [DeletedAt]      DATETIME       NULL,
    [JointAccountId] INT            NULL,
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([ReportId] ASC)
);

