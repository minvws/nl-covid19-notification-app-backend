-- TekReleaseWorkflowState
CREATE VIEW vwTekReleaseWorkflowState
AS
SELECT
	Id,
	Created,
	ValidUntil,
	LabConfirmationId,
	ConfirmationKey,
	BucketId,
	AuthorisedByCaregiver,
	DateOfSymptomsOnset,
	PollToken
FROM openjson(
	(
		SELECT 
			Id,
			Created,
			ValidUntil,
			LabConfirmationId,
			ConfirmationKey,
			BucketId,
			AuthorisedByCaregiver,
			DateOfSymptomsOnset,
			PollToken
		FROM [dbo].[TekReleaseWorkflowState]
		FOR JSON AUTO
	)
) with (
	Id int,
	Created datetime2(7),
	ValidUntil datetime2(7),
	LabConfirmationId nvarchar(6),
	ConfirmationKey varchar(max),
	BucketId varchar(max),
	AuthorisedByCaregiver datetime2(7),
	DateOfSymptomsOnset datetime2(7),
	PollToken varchar(max)
)
GO