CREATE TABLE [dbo].[Sanctions] (
    [Id]                  INT             IDENTITY (1, 1) NOT NULL,
    [Name]                NVARCHAR (250)  NOT NULL,
    [StatutoryFee]        DECIMAL (18, 2) NOT NULL,
    [ProcessingFee]       DECIMAL (18, 2) NOT NULL,
    [SanctionFee]         DECIMAL (18, 2) NOT NULL,
    [PriceByVolume]       BIT             NOT NULL,
    [ProcessingFeeByTank] BIT             NOT NULL,
    CONSTRAINT [PK_Sanctions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

