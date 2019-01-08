SELECT
    activity_id, station_id, activity_point.Long AS lon, activity_point.Lat AS lat, alt, distance
FROM (
	SELECT
        activity_id, station_id, activity_point, station_point, b.station_name, b.distance, a.alt,
        ROW_NUMBER() OVER(PARTITION BY station_id ORDER BY distance) AS num
	FROM [dbo].[activity_point] AS a
	CROSS APPLY (
		SELECT TOP 1
            a.id AS activity_id, s.id AS station_id, a.point AS activity_point, s.point AS station_point, station_name,
            s.point.STDistance(a.point) AS distance
		FROM [dbo].[station_point] AS s
		WHERE s.point.STDistance(a.point) IS NOT NULL
		ORDER BY s.point.STDistance(a.point)
	) AS b
) AS o
WHERE num=1
ORDER BY activity_id;
