CREATE TABLE [dbo].[MyDesk] (
    [DeskID]      INT           IDENTITY (1, 1) NOT NULL,
    [ProcessID]   INT           NOT NULL,
    [RequestID]   INT           NOT NULL,
    [StaffID]     INT           NOT NULL,
    [Sort]        INT           NOT NULL,
    [HasWork]     BIT           NOT NULL,
    [CreatedAt]   DATETIME      NOT NULL,
    [UpdatedAt]   DATETIME      NULL,
    [AutoPushed]  BIT           NULL,
    [HasPushed]   BIT           NOT NULL,
    [Comment]     VARCHAR (MAX) NULL,
    [FromStaffID] INT           NULL,
    [AppId]       INT           NULL,
    [Holding]     BIT           NULL,
    CONSTRAINT [PK_MyDesk_UT] PRIMARY KEY CLUSTERED ([DeskID] ASC)
);



