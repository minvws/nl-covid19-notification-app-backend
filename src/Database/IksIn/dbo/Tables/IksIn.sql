CREATE TABLE [dbo].[IksIn](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchTag] [nvarchar](20) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Content] [varbinary](max) NOT NULL,
	[Accepted] [datetime2](7) NULL,
	[Error] [bit] NOT NULL,
	CONSTRAINT [PK_IksIn] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[IksIn] ADD  CONSTRAINT [DF_IksIn_Error]  DEFAULT ((0)) FOR [Error]
GO

CREATE UNIQUE INDEX [IX_IksIn_BatchTag] ON [dbo].[IksIn] ([BatchTag])
