CREATE TABLE [dbo].[CdnStats] (
    [Id]                BIGINT        IDENTITY (1, 1) NOT NULL,
    [AverageDailyUsers] FLOAT (53)    NOT NULL,
    [FirstDate]         DATETIME2 (7) NOT NULL,
    [LastDate]          DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_CdnStats] PRIMARY KEY CLUSTERED ([Id] ASC)
);
