CREATE TABLE [dbo].[IksCreateJobInput] (
    [Id]                          BIGINT          IDENTITY (1, 1) NOT NULL,
    [DkId]                        BIGINT          NOT NULL,
    [Used]                        BIT             NOT NULL,
    [DailyKey_KeyData]            VARBINARY (MAX) NULL,
    [DailyKey_RollingStartNumber] INT             NULL,
    [DailyKey_RollingPeriod]      INT             NULL,
    [TransmissionRiskLevel]       INT             NOT NULL,
    [ReportType]                  INT             NOT NULL,
    [CountriesOfInterest]         NVARCHAR (MAX)  NOT NULL,
    [DaysSinceSymptomsOnset]      INT             NOT NULL,
    CONSTRAINT [PK_IksCreateJobInput] PRIMARY KEY CLUSTERED ([Id] ASC)
);

