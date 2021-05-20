CREATE TABLE [dbo].[TekReleaseWorkflowState] (
    [Id]                    BIGINT         IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME2 (7)  NOT NULL,
    [ValidUntil]            DATETIME2 (7)  NOT NULL,
    [LabConfirmationId]     NVARCHAR (6)   NULL, -- Obsolete after PubTek is taken in production
    [GGDKey]                NVARCHAR (7)   NULL, -- Successor of [LabConfirmationId]
    [ConfirmationKey]       VARBINARY (32) NOT NULL,
    [BucketId]              VARBINARY (32) NOT NULL,
    [AuthorisedByCaregiver] DATETIME2 (7)  NULL,
    [DateOfSymptomsOnset]   DATETIME2 (7)  NULL,
    [IsSymptomatic]         INT            NULL,
    [PollToken]             NVARCHAR (450) NULL,
    CONSTRAINT [PK_TekReleaseWorkflowState] PRIMARY KEY CLUSTERED  ([Id] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_ValidUntil]
    ON [dbo].[TekReleaseWorkflowState]([ValidUntil] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_PollToken]
    ON [dbo].[TekReleaseWorkflowState]([PollToken] ASC) WHERE ([PollToken] IS NOT NULL);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_LabConfirmationId]
    ON [dbo].[TekReleaseWorkflowState]([LabConfirmationId] ASC) WHERE ([LabConfirmationId] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_GGDKey]
    ON [dbo].[TekReleaseWorkflowState]([GGDKey] ASC) WHERE ([GGDKey] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_ConfirmationKey]
    ON [dbo].[TekReleaseWorkflowState]([ConfirmationKey] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_BucketId]
    ON [dbo].[TekReleaseWorkflowState]([BucketId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_AuthorisedByCaregiver]
    ON [dbo].[TekReleaseWorkflowState]([AuthorisedByCaregiver] ASC);

