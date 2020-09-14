CREATE TABLE [dbo].[IncomingBatchJobs] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchTag] [nvarchar](max)  NOT NULL,
	[BatchDate] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT(GETDATE()),
	[StartedOn] [datetime] NULL,
	[CompletedAt] [datetime] NULL,
	[Status] [int] NOT NULL DEFAULT(0),
	[RetryCount] [int] NOT NULL DEFAULT(0),
	[TotalKeys] [int] NULL,
	[TotalCountries] [int] NULL,
)
