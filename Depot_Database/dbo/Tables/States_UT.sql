CREATE TABLE [dbo].[States_UT] (
    [State_id]  INT NOT NULL,
    [Country_id]   INT           NOT NULL,
    [StateName]    VARCHAR (100)  NOT NULL,
    [Code]         NVARCHAR (10) NULL,
    [CreatedAt]    DATETIME      NULL,
    [UpdatedAt]    DATETIME      NULL,
    [DeleteStatus] BIT           NULL,
    [DeletedBy]    INT           NULL,
    [DeletedAt]    DATETIME      NULL, 
    CONSTRAINT [PK_States_UT] PRIMARY KEY ([State_id])
);

