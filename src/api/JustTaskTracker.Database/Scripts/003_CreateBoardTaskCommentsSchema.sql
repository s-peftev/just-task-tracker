CREATE TABLE [dbo].[BoardTaskComments]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [BoardTaskId]        UNIQUEIDENTIFIER NOT NULL,
    [AuthorId]           UNIQUEIDENTIFIER NOT NULL,
    [Body]               NVARCHAR(2000)   NOT NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [IsDeleted]          BIT              NOT NULL DEFAULT (0),
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_BoardTaskComments] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_BoardTaskComments_BoardTasks_BoardTaskId]
        FOREIGN KEY ([BoardTaskId]) REFERENCES [dbo].[BoardTasks] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_BoardTaskComments_Users_AuthorId]
        FOREIGN KEY ([AuthorId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [CK_BoardTaskComments_SoftDelete] CHECK (
        ([IsDeleted] = 0 AND [DeletedAtUtc] IS NULL) OR
        ([IsDeleted] = 1 AND [DeletedAtUtc] IS NOT NULL))
);
GO

CREATE INDEX [IX_BoardTaskComments_BoardTaskId_CreatedAtUtc_Active]
    ON [dbo].[BoardTaskComments] ([BoardTaskId], [CreatedAtUtc])
    WHERE [IsDeleted] = 0;
GO
