CREATE TABLE [dbo].[SuitabilityInspections] (
    [Id]                          INT            IDENTITY (1, 1) NOT NULL,
    [SizeOfLand]                  NVARCHAR (20)  NOT NULL,
    [ISAlongPipeLine]             BIT            NULL,
    [IsUnderHighTension]          BIT            NULL,
    [StationsWithin2KM]           INT            NULL,
    [DistanceFromExistingStation] NVARCHAR (50)  NULL,
    [IsOnHighWay]                 BIT            NULL,
    [CompanyId]                   INT            NOT NULL,
    [EIADPRStaff]                 NVARCHAR (250) NULL,
    [FacilityId]                  INT            NOT NULL,
    [ApplicationId]               INT            NOT NULL,
    CONSTRAINT [PK_SuitabilityInspections] PRIMARY KEY CLUSTERED ([Id] ASC)
);

