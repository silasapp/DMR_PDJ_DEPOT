CREATE TABLE [dbo].[TankInspections] (
    [Id]               INT      IDENTITY (1, 1) NOT NULL,
    [ApplicationId]    INT      NOT NULL,
    [NumberOfTanks]    INT      NOT NULL,
    [NumberOfPumps]    INT      NOT NULL,
    [NumberOfDriveIn]  INT      NOT NULL,
    [NumberOfDriveOut] INT      NOT NULL,
    [Date]             DATETIME NOT NULL,
    [FacilityId]       INT      NOT NULL,
    CONSTRAINT [PK_LTOs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

