CREATE TABLE [dbo].[permits] (
    [id]             INT            IDENTITY (1, 1) NOT NULL,
    [permit_no]      VARCHAR (50)   NOT NULL,
    [application_id] INT            NOT NULL,
    [company_id]     INT            NOT NULL,
    [date_issued]    DATETIME2 (0)  NOT NULL,
    [date_expire]    DATETIME2 (0)  NOT NULL,
    [categoryName]   NVARCHAR (150) NULL,
    [is_renewed]     NVARCHAR (130) NULL,
    [elps_id]        INT            NULL,
    [Printed]        BIT            NOT NULL,
    [dateString]     VARCHAR (20)   NULL,
    [ApprovedBy]     INT            NULL,
    [CreatedAt]      DATETIME       NULL,
    CONSTRAINT [PK_permit_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

