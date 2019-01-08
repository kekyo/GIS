DROP TABLE IF EXISTS [dbo].[activity_original];
CREATE TABLE [dbo].[activity_original](
	[id] [bigint] NOT NULL,
	[dateTime] [datetimeoffset](7) NOT NULL,
	[lon] [real] NOT NULL,
	[lat] [real] NOT NULL,
	[alt] [real] NOT NULL,
 CONSTRAINT [PK_activity_original] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
