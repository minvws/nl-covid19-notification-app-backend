DELETE
FROM [Workflow].[dbo].[TekReleaseWorkflowState]
WHERE ( Created < GETDATE() - 3)