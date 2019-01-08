DROP TABLE IF EXISTS [dbo].[station_original];
CREATE TABLE [dbo].[station_original](
	[id] [bigint] NOT NULL,
	[lon] [real] NOT NULL,
	[lat] [real] NOT NULL,
	[station_name] nvarchar(32) NOT NULL,
	[add] nvarchar(64) NOT NULL,
 CONSTRAINT [PK_station_original] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
