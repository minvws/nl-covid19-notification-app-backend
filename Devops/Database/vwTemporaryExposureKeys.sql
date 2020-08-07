CREATE VIEW vwTemporaryExposureKeys
AS
SELECT
	Id,
	OwnerId,
	KeyData,
	RollingStartNumber,
	RollingPeriod,
	Region,
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
			Region,
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
	Region varchar(2),
	PublishingState int,
	PublishAfter datetime2(7)
)