USE [workflow]
GO

CREATE TABLE [dbo].[KeyReleaseWorkflowStates] (
    [Id]                    BIGINT         IDENTITY (1, 1) NOT NULL,
    [Created]               DATETIME2 (7)  NOT NULL,
    [LabConfirmationId]     NVARCHAR (450) NULL,
    [PollToken]             NVARCHAR (450) NULL,
    [ConfirmationKey]       NVARCHAR (450) NULL,
    [BucketId]              NVARCHAR (450) NULL,
    [Authorised]            BIT            NOT NULL,
    [AuthorisedByCaregiver] BIT            NOT NULL,
    [ValidUntil]            DATETIME2 (7)  NOT NULL,
    [DateOfSymptomsOnset]   DATETIME2 (7)  NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_Authorised]
    ON [dbo].[KeyReleaseWorkflowStates]([Authorised] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_AuthorisedByCaregiver]
    ON [dbo].[KeyReleaseWorkflowStates]([AuthorisedByCaregiver] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_BucketId]
    ON [dbo].[KeyReleaseWorkflowStates]([BucketId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_ConfirmationKey]
    ON [dbo].[KeyReleaseWorkflowStates]([ConfirmationKey] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_LabConfirmationId]
    ON [dbo].[KeyReleaseWorkflowStates]([LabConfirmationId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_PollToken]
    ON [dbo].[KeyReleaseWorkflowStates]([PollToken] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_KeyReleaseWorkflowStates_ValidUntil]
    ON [dbo].[KeyReleaseWorkflowStates]([ValidUntil] ASC);


GO
ALTER TABLE [dbo].[KeyReleaseWorkflowStates]
    ADD CONSTRAINT [PK_KeyReleaseWorkflowStates] PRIMARY KEY CLUSTERED ([Id] ASC);


