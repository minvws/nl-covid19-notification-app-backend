SELECT LabConfirmationId, BucketId, PublishAfter, PublishingState
FROM vwTekReleaseWorkflowState wf
	LEFT JOIN vwTemporaryExposureKeys tek ON
		wf.Id = tek.OwnerId
WHERE KeyData = 'BASE64-KEY-HERE'
GO