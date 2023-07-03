CREATE TABLE [dbo].[MistdoStaff] (
    [MistdoId]       INT           IDENTITY (1, 1) NOT NULL,
    [CompanyId]      INT           NULL,
    [FacilityId]     INT           NULL,
    [MistdoServerId] VARCHAR (100) NULL,
    [FullName]       VARCHAR (200) NULL,
    [PhoneNo]        NCHAR (20)    NULL,
    [Email]          VARCHAR (100) NULL,
    [CertificateNo]  VARCHAR (100) NULL,
    [IssuedDate]     DATETIME      NULL,
    [ExpiryDate]     DATETIME      NULL,
    [CreatedAt]      DATETIME      NULL,
    [UpdatedAt]      DATETIME      NULL,
    [DeletedAt]      DATETIME      NULL,
    [DeletedStatus]  BIT           NULL,
    CONSTRAINT [PK_MistdoStaff] PRIMARY KEY CLUSTERED ([MistdoId] ASC)
);

