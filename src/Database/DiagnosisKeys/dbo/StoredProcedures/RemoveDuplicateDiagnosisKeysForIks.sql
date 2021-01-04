CREATE PROCEDURE [dbo].[RemoveDuplicateDiagnosisKeysForIks]
AS
BEGIN

	DECLARE @keyData varbinary(max);
	DECLARE @rsn int;
	DECLARE @rp int;
	
	-- Get the natural IDs of all duplicate keys; there will be one row for
	-- every duplicate DailyKey.
	DECLARE duplicates_cursor CURSOR FOR
	SELECT [DailyKey_KeyData], [DailyKey_RollingStartNumber], [DailyKey_RollingPeriod]
	FROM [dbo].[DiagnosisKeys]
	GROUP BY [DailyKey_KeyData], [DailyKey_RollingStartNumber], [DailyKey_RollingPeriod]
	HAVING count(*) > 1;

	OPEN duplicates_cursor

	FETCH NEXT FROM duplicates_cursor INTO @keyData, @rsn, @rp

	-- Iterate over the duplicate keys, handling them one-by-one; the important thing to remember
	-- here is that all of the queries must be constrained to the duplicate row of the current 
	-- iteration;  so they all share the same WHERE clause. I've put this shared WHERE clause on 
	-- one line and always on the same line - the queries here are complex and that makes them 
	-- easier to read.
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--
		-- Mark ALL as Published if any has been published
		IF EXISTS(
			SELECT 1
			FROM [dbo].[DiagnosisKeys]
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
			  AND [PublishedToEfgs] = 0x1
		)
		BEGIN
			UPDATE [dbo].[DiagnosisKeys]
			SET [PublishedToEfgs] = 0x1
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp 
		END

		--
		-- Mark all except the row with the highest TRL if non are published
		IF EXISTS(
			SELECT 1
			FROM [dbo].[DiagnosisKeys]
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
			  AND [PublishedToEfgs] = 0x0
		)
		BEGIN
			UPDATE [dbo].[DiagnosisKeys]
			SET [PublishedToEfgs] = 0x1
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
              AND Id NOT IN (
				SELECT TOP 1 Id
				FROM [dbo].[DiagnosisKeys]
				WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
					AND [Local_TransmissionRiskLevel] = (
						SELECT MAX([Local_TransmissionRiskLevel])
						FROM [dbo].[DiagnosisKeys]
						WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
				)
			)
		END

		FETCH NEXT FROM duplicates_cursor INTO @keyData, @rsn, @rp
	END

	CLOSE duplicates_cursor
	DEALLOCATE duplicates_cursor

END