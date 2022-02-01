CREATE VIEW vwTemporaryExposureKeys
AS
SELECT
	Id,
	OwnerId,
	KeyData,
	RollingStartNumber,
	RollingPeriod,
	PublishingState,
	PublishAfter
FROM openjson(
	(
		SELECT 
			Id,
			OwnerId,
			KeyData,
			RollingStartNumber,
			RollingPeriod,
			PublishingState,
			PublishAfter
		FROM [TemporaryExposureKeys]
		FOR JSON AUTO
	)
) with (
	Id int,
	OwnerId int,
	KeyData varchar(max),
	RollingStartNumber int,
	RollingPeriod int,
	PublishingState int,
	PublishAfter datetime2(7)
)
GO