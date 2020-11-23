CREATE TABLE [dbo].[IksIn] (
    [Id]       INT             IDENTITY (1, 1) NOT NULL,
    [BatchTag] NVARCHAR (MAX)  NOT NULL,
    [Created]  DATETIME2 (7)   NOT NULL,
    [Content]  VARBINARY (MAX) NOT NULL,
    [Accepted] DATETIME2 (7)   NULL,
    CONSTRAINT [PK_IksIn] PRIMARY KEY CLUSTERED ([Id] ASC)
);

