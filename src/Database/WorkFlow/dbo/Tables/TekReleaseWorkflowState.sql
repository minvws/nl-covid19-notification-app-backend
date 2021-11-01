CREATE TABLE [dbo].[TekReleaseWorkflowState] (
    [Id]                    BIGINT         IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME2 (7)  NOT NULL,
    [ValidUntil]            DATETIME2 (7)  NOT NULL,
    [GGDKey]                NVARCHAR (7)   NULL, -- Successor of [LabConfirmationId]
    [ConfirmationKey]       VARBINARY (32) NOT NULL,
    [BucketId]              VARBINARY (32) NOT NULL,
    [AuthorisedByCaregiver] DATETIME2 (7)  NULL,
    [DateOfSymptomsOnset]   DATETIME2 (7)  NULL,
    [IsSymptomatic]         INT            NULL,
    [IsOriginPortal]        BIT            NULL, -- Will be set not null default 0 in post-deploy
    CONSTRAINT [PK_TekReleaseWorkflowState] PRIMARY KEY CLUSTERED  ([Id] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_TekReleaseWorkflowState_ValidUntil]
    ON [dbo].[TekReleaseWorkflowState]([ValidUntil] ASC);

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


GO

