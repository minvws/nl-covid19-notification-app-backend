CREATE TABLE [dbo].[IksCreateJobOutput] (
    [Id]                   BIGINT          IDENTITY (1, 1) NOT NULL,
    [Created]              DATETIME2 (7)   NOT NULL,
    [Content]              VARBINARY (MAX) NOT NULL,
    [CreatingJobQualifier] INT             NOT NULL,
    CONSTRAINT [PK_IksCreateJobOutput] PRIMARY KEY CLUSTERED ([Id] ASC)
);

