BEGIN TRANSACTION;

-- Delete all rows with duplicate EXCEPT the row with the highest ID (the last duplicate)
DELETE FROM IksIn
WHERE BatchTag IN (
	SELECT BatchTag
	FROM IksIn
	GROUP BY BatchTag
	HAVING count(*) > 1	
) AND Id NOT IN (
	SELECT MAX(Id)
	FROM IksIn
	WHERE BatchTag IN (
		SELECT BatchTag
		FROM IksIn
		GROUP BY BatchTag
		HAVING count(*) > 1	
	)
);

COMMIT;
