DROP TABLE IF EXISTS [dbo].[station_point];
CREATE TABLE [dbo].[station_point](
	[id] [bigint] NOT NULL,
	[point] [geography] NOT NULL,
	[station_name] nvarchar(32) NOT NULL,
	[add] nvarchar(64) NOT NULL,
 CONSTRAINT [PK_station_point] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

-- geography座標にインデックスを設定する
CREATE SPATIAL INDEX SPATIAL_station_point ON dbo.station_point(point) USING GEOGRAPHY_GRID
	 WITH( GRIDS  = ( LEVEL_1  = MEDIUM, LEVEL_2  = MEDIUM, LEVEL_3  = MEDIUM, LEVEL_4  = MEDIUM), CELLS_PER_OBJECT  = 16, STATISTICS_NORECOMPUTE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);

-- 座標をgeography座標型に変換する (SRID: 4326 (WGS84))
INSERT INTO [dbo].[station_point]
SELECT id, geography::Point(lat, lon, 4326) as point, station_name, [add]
FROM [dbo].[station_original];
