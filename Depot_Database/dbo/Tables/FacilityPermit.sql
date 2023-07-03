CREATE TABLE [dbo].[FacilityPermit] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [PermitNo]      VARCHAR (50)  NOT NULL,
    [DateIssued]    DATE          NOT NULL,
    [DateExpired]   DATE          NOT NULL,
    [IsRenewed]     BIT           NOT NULL,
    [ElpsID]        INT           NOT NULL,
    [CompanyID]     INT           NOT NULL,
    [Type]          NCHAR (30)    NOT NULL,
    [FacilityName]  VARCHAR (100) NOT NULL,
    [PhaseName]     VARCHAR (100) NOT NULL,
    [CompanyName]   VARCHAR (100) NOT NULL,
    [ApplicationID] INT           NOT NULL,
    [CategoryID]    INT           NOT NULL,
    [FacilityID]    INT           NULL,
    CONSTRAINT [PK__Table__3214EC078ECEADD3] PRIMARY KEY CLUSTERED ([Id] ASC)
);

