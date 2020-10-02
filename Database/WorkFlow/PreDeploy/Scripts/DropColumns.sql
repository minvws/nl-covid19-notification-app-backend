-- Drop "TeksTouched" column (and corresponding constraint) from the TekReleaseWorkflowState table

ALTER TABLE [dbo].[TekReleaseWorkflowState] DROP CONSTRAINT IF EXISTS [DF_TekReleaseWorkflowState_TeksTouched];

ALTER TABLE [dbo].[TekReleaseWorkflowState] DROP COLUMN IF EXISTS [TeksTouched];