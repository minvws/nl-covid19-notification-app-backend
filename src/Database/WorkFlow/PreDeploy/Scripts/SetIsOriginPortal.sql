-- Sets IsOriginPortal on existing entries to 1 if their values are not defined

USE [workflow]

IF NOT EXISTS (
  SELECT 1
  FROM sys.columns
  WHERE object_id = OBJECT_ID(N'[dbo].[TekReleaseWorkflowState]')
    AND name = 'IsOriginPortal'
)
BEGIN
	ALTER TABLE [dbo].[TekReleaseWorkflowState]
	ADD [IsOriginPortal] BIT NULL

	EXEC('
		UPDATE [dbo].[TekReleaseWorkflowState]
		SET [IsOriginPortal] = 1
		WHERE [IsOriginPortal] IS NULL
	') --required due to the column not being recognised and go-statements not being allowed in begin-end blocks

	ALTER TABLE [dbo].[TekReleaseWorkflowState]
	ALTER COLUMN [IsOriginPortal] BIT NOT NULL

	ALTER TABLE [dbo].[TekReleaseWorkflowState]
ADD CONSTRAINT OriginConstraint DEFAULT 0 FOR [IsOriginPortal]
END
