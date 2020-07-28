
CREATE TABLE [dbo].[EksCreateJobOutput] (
    [Id]                   INT             NOT NULL,
    [Release]              DATETIME2 (7)   NOT NULL,
    [Region]               NVARCHAR (MAX)  NOT NULL,
    [Content]              VARBINARY (MAX) NULL,
    [CreatingJobName]      NVARCHAR (MAX)  NOT NULL,
    [CreatingJobQualifier] INT             NOT NULL
);
