CREATE TABLE [dbo].[addresses] (
    [id]          INT            IDENTITY (1, 1) NOT NULL,
    [address_1]   NVARCHAR (128) NOT NULL,
    [address_2]   NVARCHAR (128) NULL,
    [city]        NVARCHAR (128) NOT NULL,
    [postal_code] NVARCHAR (10)  NULL,
    [country_id]  INT            NOT NULL,
    [elps_id]     INT            NULL,
    [StateId]     INT            NOT NULL,
    [LgaId]       INT            NULL,
    CONSTRAINT [PK_address_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

