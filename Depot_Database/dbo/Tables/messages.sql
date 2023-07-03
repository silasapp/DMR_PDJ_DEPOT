CREATE TABLE [dbo].[messages] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [subject]    VARCHAR (250)  NULL,
    [content]    VARCHAR (MAX)  NULL,
    [read]       INT            NULL,
    [company_id] INT            NULL,
    [sender_id]  NVARCHAR (200) NULL,
    [date]       DATETIME2       NULL,
    [AppId]      INT            NULL,
    [UserID]     INT            NULL,
    [UserType]   NCHAR (10)     NULL,
    CONSTRAINT [PK_message_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

