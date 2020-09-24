CREATE TABLE [dbo].[DiagnosisKeys] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[KeyData] [varbinary](32) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[Origin] [varchar](2) NOT NULL,
	[CountriesOfInterest] [varchar](MAX) NOT NULL,
	[TransmissionRiskLevel] [int] NOT NULL,
	[PublishedToEksOn] [datetime] NULL,
	[PublishedToInteropOn] [datetime] NULL,
	[IsStuffing] [bit] NOT NULL,
	CONSTRAINT [PK_DiagnosisKeys] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
