CREATE TABLE [dbo].[branches] (
    [id]          INT            IDENTITY (2, 1) NOT NULL,
    [name]        NVARCHAR (128) NOT NULL,
    [branchCode]  NVARCHAR (128) NOT NULL,
    [location]    NVARCHAR (128) NOT NULL,
    [create_at]   DATETIME       NOT NULL,
    [lastedit_at] DATETIME       NOT NULL,
    [status]      INT            NOT NULL,
    [Address]     NVARCHAR (250) NULL,
    [State_id]    INT            NULL,
    CONSTRAINT [PK_branch_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

