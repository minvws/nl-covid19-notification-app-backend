﻿CREATE TABLE [dbo].[IksOut](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[ValidFor] [datetime2](7) NOT NULL,
	[Content] [varbinary](max) NOT NULL,
	[Sent] [bit] NOT NULL,
	[Qualifier] [int] NOT NULL,
    [Error] BIT NOT NULL DEFAULT 0, 
    [ProcessState] NVARCHAR(50) NOT NULL DEFAULT 'New', 
    [RetryCount] INT NOT NULL DEFAULT 0, 
    [CanRetry] BIT NULL, 
    CONSTRAINT [PK_IksOut] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

