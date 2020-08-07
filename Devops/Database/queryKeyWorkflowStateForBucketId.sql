SELECT LabConfirmationId, KeyData, PublishAfter, PublishingState
FROM vwTekReleaseWorkflowState wf
	LEFT JOIN vwTemporaryExposureKeys tek ON
		wf.Id = tek.OwnerId
WHERE BucketId = 'BASE-64-BUCKET-ID-HERE'
GO
