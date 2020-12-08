BEGIN TRANSACTION;

SELECT count(distinct BatchTag)
FROM IksIn

-- Delete all rows with duplicate EXCEPT the row with the highest ID (the last duplicate)
DELETE FROM IksIn
WHERE BatchTag IN (
	SELECT BatchTag
	FROM IksIn
	GROUP BY BatchTag
	HAVING count(*) > 1	
) AND Id NOT IN (
	SELECT MaxId
	FROM (
		SELECT BatchTag, MAX(Id) as MaxId
		FROM IksIn
		GROUP BY BatchTag
	) x
);


SELECT count(distinct BatchTag)
FROM IksIn

ROLLBACK

--COMMIT;
