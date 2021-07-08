CREATE TABLE [dbo].[EksCreateJobInput](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TekId] [bigint] NULL,
	[Used] [bit] NOT NULL,
	[KeyData] [varbinary](900) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[TransmissionRiskLevel] [int] NOT NULL,
	[DaysSinceSymptomsOnset] [int] NULL,
	[Symptomatic] [int] NULL,
    [ReportType] [int] NULL, 
    CONSTRAINT [PK_EksCreateJobInput] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[EksCreateJobInput] ADD  CONSTRAINT [DF_EksCreateJobInput_TransmissionRiskLevel]  DEFAULT ((0)) FOR [TransmissionRiskLevel]
GO

ALTER TABLE [dbo].[EksCreateJobInput] ADD  CONSTRAINT [DF_EksCreateJobInput_DaysSinceSymptomsOnset]  DEFAULT ((0)) FOR [DaysSinceSymptomsOnset]
GO

ALTER TABLE [dbo].[EksCreateJobInput] ADD  CONSTRAINT [DF_EksCreateJobInput_ReportType]  DEFAULT ((1)) FOR [ReportType]
GO

CREATE NONCLUSTERED INDEX [IX_EksCreateJobInput_TransmissionRiskLevel]
    ON [dbo].[EksCreateJobInput]([TransmissionRiskLevel] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EksCreateJobInput_TekId]
    ON [dbo].[EksCreateJobInput]([TekId] ASC) WHERE ([TekId] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_EksCreateJobInput_KeyData]
    ON [dbo].[EksCreateJobInput]([KeyData] ASC);