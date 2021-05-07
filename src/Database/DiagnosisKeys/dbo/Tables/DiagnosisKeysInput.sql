CREATE TABLE [dbo].[DiagnosisKeysInput](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TekId] [bigint] NOT NULL,
	[DailyKey_KeyData] [varbinary](max) NULL,
	[DailyKey_RollingStartNumber] [int] NULL,
	[DailyKey_RollingPeriod] [int] NULL,
	[Local_TransmissionRiskLevel] [int] NULL,
	[Local_DaysSinceSymptomsOnset] [int] NULL,
	[Local_Symptomatic] [int] NULL,
 [Local_ReportType] INT NULL, 
    CONSTRAINT [PK_DiagnosisKeysInput] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


