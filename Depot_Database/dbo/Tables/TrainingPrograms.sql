CREATE TABLE [dbo].[TrainingPrograms] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [StaffName]   NVARCHAR (250) NULL,
    [Role]        NVARCHAR (250) NULL,
    [CourseTitle] NVARCHAR (250) NULL,
    [Duration]    NVARCHAR (250) NULL,
    [StartDate]   DATETIME       NULL,
    CONSTRAINT [PK_TrainingProgram] PRIMARY KEY CLUSTERED ([Id] ASC)
);

