CREATE TABLE [dbo].[BoardTaskAttachments]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [BoardTaskId]        UNIQUEIDENTIFIER NOT NULL,
    [UploadedById]       UNIQUEIDENTIFIER NOT NULL,
    [OriginalFileName]   NVARCHAR(255)    NOT NULL,
    [ContentType]        NVARCHAR(127)    NOT NULL,
    [FileSizeBytes]      BIGINT           NOT NULL,
    [BlobName]           NVARCHAR(500)    NOT NULL,
    [Position]           INT              NOT NULL DEFAULT (0),
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,

    CONSTRAINT [PK_BoardTaskAttachments] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_BoardTaskAttachments_BoardTasks_BoardTaskId]
        FOREIGN KEY ([BoardTaskId]) REFERENCES [dbo].[BoardTasks] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [FK_BoardTaskAttachments_Users_UploadedById]
        FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [CK_BoardTaskAttachments_FileSizeBytes_Positive] CHECK ([FileSizeBytes] > 0)
);
GO

CREATE UNIQUE INDEX [UQ_BoardTaskAttachments_BoardTaskId_Position]
    ON [dbo].[BoardTaskAttachments] ([BoardTaskId], [Position]);
GO

CREATE UNIQUE INDEX [UQ_BoardTaskAttachments_BlobName]
    ON [dbo].[BoardTaskAttachments] ([BlobName]);
GO
