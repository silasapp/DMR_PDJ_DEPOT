CREATE TABLE [dbo].[Waivers] (
    [ID]                  INT            IDENTITY (1, 1) NOT NULL,
    [ApplicationId]       INT            NOT NULL,
    [RequestFrom]         NVARCHAR (250) NULL,
    [RequestReason]       NVARCHAR (MAX) NOT NULL,
    [AssignedManager]     NVARCHAR (250) NOT NULL,
    [Approved]            BIT            NULL,
    [ManagerReason]       NVARCHAR (MAX) NULL,
    [WaiverFor]           NVARCHAR (50)  NULL,
    [RequestDate]         DATETIME       NULL,
    [ManagerResponseDate] DATETIME       NULL,
    [entityId]            INT            NULL,
    CONSTRAINT [PK_Waiver] PRIMARY KEY CLUSTERED ([ID] ASC)
);

