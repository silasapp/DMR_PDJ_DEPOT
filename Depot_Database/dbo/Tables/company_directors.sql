CREATE TABLE [dbo].[company_directors] (
    [id]          INT          IDENTITY (5934, 1) NOT NULL,
    [company_id]  INT          NOT NULL,
    [firstname]   VARCHAR (50) NOT NULL,
    [lastname]    VARCHAR (50) NOT NULL,
    [address_id]  INT          NOT NULL,
    [telephone]   VARCHAR (20) NOT NULL,
    [nationality] INT          NOT NULL,
    [elps_id]     INT          NULL,
    CONSTRAINT [PK_company_director_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

