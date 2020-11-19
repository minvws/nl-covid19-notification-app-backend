CREATE TABLE [dbo].[TemporaryExposureKeys] (
    [Id]                 BIGINT         IDENTITY (1, 1) NOT NULL,
    [OwnerId]            BIGINT         NOT NULL,
    [KeyData]            VARBINARY (32) NOT NULL,
    [RollingStartNumber] INT            NOT NULL,
    [RollingPeriod]      INT            NOT NULL,
    [PublishingState]    INT            NOT NULL,
    [PublishAfter]       DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_TemporaryExposureKeys] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TemporaryExposureKeys_TekReleaseWorkflowState_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[TekReleaseWorkflowState] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_PublishingState]
    ON [dbo].[TemporaryExposureKeys]([PublishingState] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_PublishAfter]
    ON [dbo].[TemporaryExposureKeys]([PublishAfter] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_OwnerId]
    ON [dbo].[TemporaryExposureKeys]([OwnerId] ASC);

