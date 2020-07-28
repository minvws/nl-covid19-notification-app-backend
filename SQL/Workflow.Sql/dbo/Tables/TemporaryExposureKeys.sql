CREATE TABLE [dbo].[TemporaryExposureKeys] (
    [Id]                    BIGINT          IDENTITY (1, 1) NOT NULL,
    [OwnerId]               BIGINT          NOT NULL,
    [KeyData]               VARBINARY (MAX) NOT NULL,
    [RollingStartNumber]    INT             NOT NULL,
    [RollingPeriod]         INT             NOT NULL,
    [TransmissionRiskLevel] INT             NOT NULL,
    [Region]                NVARCHAR (450)  NOT NULL,
    [PublishingState]       INT             NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_OwnerId]
    ON [dbo].[TemporaryExposureKeys]([OwnerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_PublishingState]
    ON [dbo].[TemporaryExposureKeys]([PublishingState] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TemporaryExposureKeys_Region]
    ON [dbo].[TemporaryExposureKeys]([Region] ASC);


GO
ALTER TABLE [dbo].[TemporaryExposureKeys]
    ADD CONSTRAINT [PK_TemporaryExposureKeys] PRIMARY KEY CLUSTERED ([Id] ASC);


GO
ALTER TABLE [dbo].[TemporaryExposureKeys]
    ADD CONSTRAINT [FK_TemporaryExposureKeys_KeyReleaseWorkflowStates_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[KeyReleaseWorkflowStates] ([Id]) ON DELETE CASCADE;


