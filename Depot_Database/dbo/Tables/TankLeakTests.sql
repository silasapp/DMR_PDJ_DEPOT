CREATE TABLE [dbo].[TankLeakTests] (
    [Id]                 INT      IDENTITY (1, 1) NOT NULL,
    [DateAdded]          DATETIME NOT NULL,
    [ApplicationId]      INT      NOT NULL,
    [FacilityId]         INT      NOT NULL,
    [TanksTotalCapacity] INT      NOT NULL,
    CONSTRAINT [PK_TankLeakTests] PRIMARY KEY CLUSTERED ([Id] ASC)
);

