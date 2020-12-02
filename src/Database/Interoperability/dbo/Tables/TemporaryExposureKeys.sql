CREATE TABLE [dbo].[TemporaryExposureKeys] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IncomingBatchJobId] [bigint] NOT NULL,
	[KeyData] [varbinary](32) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[Origin] [varchar](2) NOT NULL,
	[CountriesOfInterest] [varchar](MAX) NOT NULL,
	[OriginTransmissionRiskLevel] [int] NOT NULL,
	[TransmissionRiskLevel] [int] NOT NULL,
	[ProcessingState] [int] NOT NULL,
	CONSTRAINT [PK_ExternalTemporaryExposureKeys] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)