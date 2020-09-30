-- Drop "TeksTouched" column from the TekReleaseWorkflowState table

IF EXISTS(SELECT * FROM SYS.columns WHERE name='TeksTouched' AND  OBJECT_ID = OBJECT_ID('[dbo].[TekReleaseWorkflowState]'))
	ALTER TABLE [dbo].[TekReleaseWorkflowState] DROP COLUMN [TeksTouched];