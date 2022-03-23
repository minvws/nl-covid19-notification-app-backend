CREATE TABLE [dbo].[DashboardOutputJson] (
    [Id]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [CreatedDate] DATETIME2 (7)  NOT NULL,
    [JsonData]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_DashboardOutputJson] PRIMARY KEY CLUSTERED ([Id] ASC)
);
