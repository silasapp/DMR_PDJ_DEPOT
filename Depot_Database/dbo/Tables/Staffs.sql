CREATE TABLE [dbo].[Staffs] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [FirstName] NVARCHAR (50)  NULL,
    [LastName]  NVARCHAR (50)  NULL,
    [UserId]    NVARCHAR (50)  NOT NULL,
    [Email]     NVARCHAR (250) NOT NULL,
    [PhoneNo]   NVARCHAR (50)  NULL,
    [Signature] NVARCHAR (250) NULL,
    CONSTRAINT [PK_Staffs_Old] PRIMARY KEY CLUSTERED ([Id] ASC)
);

