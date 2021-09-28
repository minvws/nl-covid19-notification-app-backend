CREATE TABLE [dbo].[DiagnosisKeys](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PublishedLocally] [bit] NOT NULL,
	[DailyKey_KeyData] [varbinary](max) NULL,
	[DailyKey_RollingStartNumber] [int] NULL,
	[DailyKey_RollingPeriod] [int] NULL,
	[Origin] [int] NOT NULL,
	[Local_TransmissionRiskLevel] [int] NULL,
	[Local_DaysSinceSymptomsOnset] [int] NULL,
	[Local_Symptomatic] [int] NULL,
	[PublishedToEfgs] [bit] NOT NULL,
	[Efgs_BatchTag] [nvarchar](max) NULL,
	[Efgs_CountriesOfInterest] [nvarchar](max) NULL,
	[Efgs_TransmissionRiskLevel] [int] NULL,
	[Efgs_DaysSinceSymptomsOnset] [int] NULL,
	[Efgs_ReportType] [int] NULL,
	[Efgs_CountryOfOrigin] [nvarchar](2) NULL,
	[Created] [datetime2](7) NULL,
	[ReadyForCleanup] [bit] NULL,
 [Local_ReportType] INT NULL, 
    CONSTRAINT [PK_DiagnosisKeys] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [Symptom_ReportType] ON [dbo].[DiagnosisKeys]
(
	[Local_Symptomatic] ASC,
	[Local_ReportType] ASC
)
INCLUDE([Id],[Local_TransmissionRiskLevel],[Local_DaysSinceSymptomsOnset]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DiagnosisKeys] ADD  CONSTRAINT [DF_DiagnosisKeys_Created]  DEFAULT (getdate()) FOR [Created]
GO


