CREATE TABLE [dbo].[Content] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Created]         DATETIME2 (7)   NOT NULL,
    [Release]         DATETIME2 (7)   NOT NULL,
    [PublishingId]    NVARCHAR (64)   NOT NULL,
    [Content]         VARBINARY (MAX) NOT NULL,
    [ContentTypeName] NVARCHAR (450)  NOT NULL,
    [Type]            NVARCHAR (450)  NOT NULL,
    CONSTRAINT [PK_Content] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Content_Type]
    ON [dbo].[Content]([Type] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_Release]
    ON [dbo].[Content]([Release] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_PublishingId]
    ON [dbo].[Content]([PublishingId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_ContentTypeName]
    ON [dbo].[Content]([ContentTypeName] ASC);

