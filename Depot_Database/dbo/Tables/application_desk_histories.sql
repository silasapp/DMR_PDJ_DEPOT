CREATE TABLE [dbo].[application_desk_histories] (
    [id]             INT            IDENTITY (10849, 1) NOT NULL,
    [application_id] INT            NOT NULL,
    [UserName]       NVARCHAR (200) NOT NULL,
    [status]         NVARCHAR (50)  NOT NULL,
    [comment]        VARCHAR (MAX)  NOT NULL,
    [date]           DATETIME2 (0)  NOT NULL,
    [StaffID]        INT            NULL,
    CONSTRAINT [PK_application_desk_history_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

