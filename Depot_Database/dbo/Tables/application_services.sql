CREATE TABLE [dbo].[application_services] (
    [Id]             INT IDENTITY (1, 1) NOT NULL,
    [application_id] INT NOT NULL,
    [service_id]     INT NOT NULL,
    CONSTRAINT [PK_application_service_application_id] PRIMARY KEY CLUSTERED ([Id] ASC)
);

