CREATE TABLE [dbo].[Content] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Created]         DATETIME2 (7)   NOT NULL,
    [Release]         DATETIME2 (7)   NOT NULL,
    [PublishingId]    NVARCHAR (64)   NULL,
    [Content]         VARBINARY (MAX) NULL,
    [ContentTypeName] NVARCHAR (450)  NULL,
    [Type]            NVARCHAR (450)  NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Content_ContentTypeName]
    ON [dbo].[Content]([ContentTypeName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_PublishingId]
    ON [dbo].[Content]([PublishingId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_Release]
    ON [dbo].[Content]([Release] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Content_Type]
    ON [dbo].[Content]([Type] ASC);


GO
ALTER TABLE [dbo].[Content]
    ADD CONSTRAINT [PK_Content] PRIMARY KEY CLUSTERED ([Id] ASC);
