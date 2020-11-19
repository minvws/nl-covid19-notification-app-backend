CREATE TABLE [dbo].[DiagnosisKeys] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PublishedLocally] [bit] NOT NULL,
	[DailyKey_KeyData] [varbinary](max) NULL,
	[DailyKey_RollingStartNumber] [int] NULL,
	[DailyKey_RollingPeriod] [int] NULL,
	[Origin] [int] NOT NULL,
	[Local_TransmissionRiskLevel] [int] NULL,
	[Local_DaysSinceSymptomsOnset] [int] NULL,
	[PublishedToEfgs] [bit] NOT NULL,
	[Efgs_BatchTag] [nvarchar](max) NULL,
	[Efgs_CountriesOfInterest] [nvarchar](max) NULL,
	[Efgs_TransmissionRiskLevel] [int] NULL,
	[Efgs_DaysSinceSymptomsOnset] [int] NULL,
	[Efgs_ReportType] [int] NULL,
 CONSTRAINT [PK_DiagnosisKeys] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

