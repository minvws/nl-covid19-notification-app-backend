-- TekReleaseWorkflowState
CREATE VIEW vwTekReleaseWorkflowState
AS
SELECT
	Id,
	Created,
	ValidUntil,
	GGDKey,
	ConfirmationKey,
	BucketId,
	AuthorisedByCaregiver,
	DateOfSymptomsOnset,
	IsSymptomatic,
	IsOriginPortal
FROM openjson(
	(
		SELECT 
			Id,
			Created,
			ValidUntil,
			GGDKey,
			ConfirmationKey,
			BucketId,
			AuthorisedByCaregiver,
			DateOfSymptomsOnset,
			IsSymptomatic,
			IsOriginPortal
		FROM [dbo].[TekReleaseWorkflowState]
		FOR JSON AUTO
	)
) with (
	Id int,
	Created datetime2(7),
	ValidUntil datetime2(7),
	GGDKey nvarchar(6),
	ConfirmationKey varchar(max),
	BucketId varchar(max),
	AuthorisedByCaregiver datetime2(7),
	DateOfSymptomsOnset datetime2(7),
	IsSymptomatic int,
	IsOriginPortal bit
)
GO