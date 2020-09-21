CREATE TABLE [dbo].[InteropKeySet] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Created]         DATETIME2 (7)   NOT NULL,
    [Content]         VARBINARY (MAX) NOT NULL
    CONSTRAINT [PK_InteropKeySet] PRIMARY KEY CLUSTERED ([Id] ASC)
);
