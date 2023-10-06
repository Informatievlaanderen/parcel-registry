CREATE TABLE [geolocation].[ParcelOsloGeolocation](
	[IDENTIFICATOR_ID] [varchar](max) NULL,
	[IDENTIFICATOR_NAAMRUIMTE] [varchar](max) NULL,
	[IDENTIFICATOR_OBJECTID] [varchar](max) NULL,
	[IDENTIFICATOR_VERSIEID] [varchar](max) NULL,
	[PERCEELSTATUS] [varchar](max) NULL,
	[ADRESSEN_OBJECTIDS] [varchar](max) NULL,
	[REMOVED] [bit] NULL,
	[msgkey] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[msgkey] ASC
))