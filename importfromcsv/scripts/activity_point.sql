DROP TABLE IF EXISTS [dbo].[activity_point];
CREATE TABLE [dbo].[activity_point](
	[id] [bigint] NOT NULL,
	[dateTime] [datetimeoffset](7) NOT NULL,
	[point] [geography] NOT NULL,
	[alt] [real] NOT NULL,
 CONSTRAINT [PK_activity_point] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

-- geography座標にインデックスを設定する
CREATE SPATIAL INDEX SPATIAL_activity_point ON dbo.activity_point(point) USING GEOGRAPHY_GRID
	 WITH( GRIDS  = ( LEVEL_1  = MEDIUM, LEVEL_2  = MEDIUM, LEVEL_3  = MEDIUM, LEVEL_4  = MEDIUM), CELLS_PER_OBJECT  = 16, STATISTICS_NORECOMPUTE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

-- 座標をgeography座標型に変換する (SRID: 4326 (WGS84))
INSERT INTO [dbo].[activity_point]
SELECT id, dateTime, geography::Point(lat, lon, 4326) as point, alt
FROM [dbo].[activity_original];
