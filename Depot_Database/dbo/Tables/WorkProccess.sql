﻿CREATE TABLE [dbo].[WorkProccess] (
    [ProccessID]        INT          IDENTITY (1, 1) NOT NULL,
    [PhaseID]           INT          NOT NULL,
    [CategoryID]        INT          NOT NULL,
    [RoleID]            INT          NOT NULL,
    [Sort]              INT          NOT NULL,
    [LocationID]        INT          NOT NULL,
    [canPush]           BIT          NOT NULL,
    [canWork]           BIT          NOT NULL,
    [canInspect]        BIT          NOT NULL,
    [canPrint]          BIT          NULL,
    [canSchdule]        BIT          NOT NULL,
    [canReport]         BIT          NOT NULL,
    [canAccept]         BIT          NOT NULL,
    [canReject]         BIT          NOT NULL,
    [CreatedAt]         DATETIME     NOT NULL,
    [CreatedBy]         INT          NULL,
    [UpdatedAt]         DATETIME     NULL,
    [UpdatedBy]         INT          NULL,
    [DeleteStatus]      BIT          NOT NULL,
    [DeletedBy]         INT          NULL,
    [DeletedAt]         DATETIME     NULL,
    [ModificationStage] VARCHAR (30) NULL,
    CONSTRAINT [PK_WorkProccess_] PRIMARY KEY CLUSTERED ([ProccessID] ASC)
);

