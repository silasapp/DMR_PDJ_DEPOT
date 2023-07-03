CREATE TABLE [dbo].[ATGs] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [FacilityId]  INT            NOT NULL,
    [Available]   BIT            NOT NULL,
    [Parameters]  NVARCHAR (250) NOT NULL,
    [Functional]  BIT            NOT NULL,
    [DPROfficial] NVARCHAR (250) NULL,
    CONSTRAINT [PK_ATGs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

