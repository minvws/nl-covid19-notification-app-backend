CREATE TABLE [dbo].[EksCreateJobInput](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TekId] [bigint] NULL,
	[Used] [bit] NOT NULL,
	[KeyData] [varbinary](900) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[DaysSinceSymptomsOnset] [int] NOT NULL DEFAULT 0,
	[IsInfectious] [bit] NOT NULL DEFAULT 0,
 CONSTRAINT [PK_EksCreateJobInput] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EksCreateJobInput_TekId]
    ON [dbo].[EksCreateJobInput]([TekId] ASC) WHERE ([TekId] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_EksCreateJobInput_KeyData]
    ON [dbo].[EksCreateJobInput]([KeyData] ASC);