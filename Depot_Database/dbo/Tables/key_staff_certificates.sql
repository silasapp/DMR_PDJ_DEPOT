CREATE TABLE [dbo].[key_staff_certificates] (
    [id]                INT            IDENTITY (6122, 1) NOT NULL,
    [company_key_staff] INT            NOT NULL,
    [name]              VARCHAR (100)  NOT NULL,
    [cert_no]           VARCHAR (50)   NOT NULL,
    [year]              INT            NOT NULL,
    [issuer]            NVARCHAR (100) NULL,
    [Elps_Id]           INT            NULL,
    CONSTRAINT [PK_key_staff_certificate_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

