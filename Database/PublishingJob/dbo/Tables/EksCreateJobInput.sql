CREATE TABLE [dbo].[EksCreateJobInput] (
    [Id]                    BIGINT          IDENTITY (1, 1) NOT NULL,
    [TekId]                 BIGINT          NULL,
    [Used]                  BIT             NOT NULL,
    [KeyData]               VARBINARY (900) NOT NULL,
    [RollingStartNumber]    INT             NOT NULL,
    [RollingPeriod]         INT             NOT NULL,
    [TransmissionRiskLevel] INT             NOT NULL,
    CONSTRAINT [PK_EksCreateJobInput] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EksCreateJobInput_TransmissionRiskLevel]
    ON [dbo].[EksCreateJobInput]([TransmissionRiskLevel] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_EksCreateJobInput_TekId]
    ON [dbo].[EksCreateJobInput]([TekId] ASC) WHERE ([TekId] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_EksCreateJobInput_KeyData]
    ON [dbo].[EksCreateJobInput]([KeyData] ASC);

