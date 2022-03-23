CREATE TABLE [dbo].[DashboardInputJson] (
    [Id]             BIGINT         IDENTITY (1, 1) NOT NULL,
    [DownloadedDate] DATETIME2 (7)  NULL,
    [ProcessedDate]  DATETIME2 (7)  NULL,
    [Hash]           NVARCHAR (450) NULL,
    [JsonData]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_DashboardInputJson] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_DashboardInputJson_Hash]
    ON [dbo].[DashboardInputJson]([Hash] ASC);
