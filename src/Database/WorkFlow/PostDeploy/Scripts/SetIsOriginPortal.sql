-- Sets IsOriginPortal on existing entries to 1 if their values are not defined

USE [workflow]

UPDATE [dbo].[TekReleaseWorkflowState]
SET [IsOriginPortal] = 1
WHERE [IsOriginPortal] IS NULL
GO

ALTER TABLE [dbo].[TekReleaseWorkflowState]
ALTER COLUMN [IsOriginPortal] BIT NOT NULL
GO

ALTER TABLE [dbo].[TekReleaseWorkflowState]
ADD CONSTRAINT IsOriginConstraint DEFAULT 0 FOR [IsOriginPortal]
GO