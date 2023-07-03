CREATE TABLE [dbo].[countries] (
    [id]                INT            NOT NULL,
    [name]              NVARCHAR (128) NOT NULL,
    [iso_code_2]        NVARCHAR (2)   NOT NULL,
    [iso_code_3]        NVARCHAR (3)   NOT NULL,
    [address_format]    NVARCHAR (MAX) NOT NULL,
    [postcode_required] SMALLINT       NOT NULL,
    [status]            SMALLINT       NOT NULL,
    CONSTRAINT [PK_country_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

