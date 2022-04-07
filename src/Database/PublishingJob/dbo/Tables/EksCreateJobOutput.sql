CREATE TABLE [dbo].[EksCreateJobOutput] (
    [Id]                   INT             IDENTITY (1, 1) NOT NULL,
    [Release]              DATETIME2 (7)   NOT NULL,
    [Region]               NVARCHAR (MAX)  NOT NULL,
    [Content]              VARBINARY (MAX) NOT NULL,
    [CreatingJobName]      NVARCHAR (MAX)  NOT NULL,
    [CreatingJobQualifier] INT             NOT NULL,
    [GaenVersion]          INT             NOT NULL  DEFAULT 12,
    [OutputId]             NVARCHAR (36)   NULL,
    CONSTRAINT [PK_EksCreateJobOutput] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EksCreateJobOutput_OutputId_GaenVersion] ON [dbo].[EksCreateJobOutput]
(
	[OutputId] ASC,
	[GaenVersion] ASC
)
WHERE ([OutputId] IS NOT NULL)

