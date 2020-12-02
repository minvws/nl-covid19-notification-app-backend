CREATE TABLE [dbo].[EksCreateJobOutput] (
    [Id]                   INT             IDENTITY (1, 1) NOT NULL,
    [Release]              DATETIME2 (7)   NOT NULL,
    [Region]               NVARCHAR (MAX)  NOT NULL,
    [Content]              VARBINARY (MAX) NOT NULL,
    [CreatingJobName]      NVARCHAR (MAX)  NOT NULL,
    [CreatingJobQualifier] INT             NOT NULL,
    CONSTRAINT [PK_EksCreateJobOutput] PRIMARY KEY CLUSTERED ([Id] ASC)
);

