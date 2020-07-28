CREATE TABLE [dbo].[EksCreateJobInput] (
    [Id]                    BIGINT          NOT NULL,
    [Used]                  BIT             NOT NULL,
    [KeyData]               VARBINARY (MAX) NOT NULL,
    [RollingStartNumber]    INT             NOT NULL,
    [RollingPeriod]         INT             NOT NULL,
    [TransmissionRiskLevel] INT             NOT NULL
);
