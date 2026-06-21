ALTER TABLE [dbo].[BoardTaskAttachments]
ADD
    [IsDeleted]          BIT              NOT NULL DEFAULT (0),
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL;
GO

ALTER TABLE [dbo].[BoardTaskAttachments]
ADD CONSTRAINT [CK_BoardTaskAttachments_SoftDelete] CHECK (
    ([IsDeleted] = 0 AND [DeletedAtUtc] IS NULL) OR
    ([IsDeleted] = 1 AND [DeletedAtUtc] IS NOT NULL));
GO

DROP INDEX [UQ_BoardTaskAttachments_BoardTaskId_Position]
    ON [dbo].[BoardTaskAttachments];
GO

DROP INDEX [UQ_BoardTaskAttachments_BlobName]
    ON [dbo].[BoardTaskAttachments];
GO

CREATE UNIQUE INDEX [UQ_BoardTaskAttachments_BoardTaskId_Position_Active]
    ON [dbo].[BoardTaskAttachments] ([BoardTaskId], [Position])
    WHERE [IsDeleted] = 0;
GO

CREATE UNIQUE INDEX [UQ_BoardTaskAttachments_BlobName_Active]
    ON [dbo].[BoardTaskAttachments] ([BlobName])
    WHERE [IsDeleted] = 0;
GO
