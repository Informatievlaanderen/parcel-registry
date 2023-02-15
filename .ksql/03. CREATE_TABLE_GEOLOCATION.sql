-- execute before creating connector
-- Needed to do this manually as connector auto-creates table with varchar as primary key
-- BUT the connector INSERTS/UPDATES with nvarchar! => CAST is needed and expensive!
CREATE TABLE [geolocation].[ParcelOsloGeolocation](
	[IDENTIFICATOR_ID] [varchar](max) NULL,
	[IDENTIFICATOR_NAAMRUIMTE] [varchar](max) NULL,
	[IDENTIFICATOR_OBJECTID] [varchar](max) NULL,
	[IDENTIFICATOR_VERSIEID] [varchar](max) NULL,
	[PERCEELSTATUS] [varchar](max) NULL,
	[ADRESSEN] [varchar](max) NULL,
	[REMOVED] [bit] NULL,
	[msgkey] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED ([msgkey] ASC)
WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO