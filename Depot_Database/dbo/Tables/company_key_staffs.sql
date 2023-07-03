CREATE TABLE [dbo].[company_key_staffs] (
    [id]                    INT           IDENTITY (5555, 1) NOT NULL,
    [company_id]            INT           NOT NULL,
    [firstname]             VARCHAR (50)  NOT NULL,
    [lastname]              VARCHAR (50)  NOT NULL,
    [nationality]           VARCHAR (50)  NOT NULL,
    [designation]           VARCHAR (50)  NOT NULL,
    [qualification]         VARCHAR (MAX) NOT NULL,
    [years_of_exp]          INT           NOT NULL,
    [skills]                VARCHAR (MAX) NULL,
    [training_certificates] VARCHAR (MAX) NULL,
    [Elps_Id]               INT           NULL,
    CONSTRAINT [PK_company_key_staff_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

