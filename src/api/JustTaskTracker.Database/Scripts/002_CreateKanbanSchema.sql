CREATE TABLE [dbo].[Boards]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [Name]               NVARCHAR(100)    NOT NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Boards] PRIMARY KEY CLUSTERED ([Id])
);
GO

CREATE TABLE [dbo].[BoardMembers]
(
    [BoardId]            UNIQUEIDENTIFIER NOT NULL,
    [UserId]             UNIQUEIDENTIFIER NOT NULL,
    [JoinedAtUtc]        DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [Role]               TINYINT          NOT NULL, -- 1 = Owner, 2 = Admin, 3 = User, 4 = Viewer

    CONSTRAINT [PK_BoardMembers] PRIMARY KEY CLUSTERED ([BoardId], [UserId]),
    CONSTRAINT [FK_BoardMembers_Boards_BoardId]
        FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_BoardMembers_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [UQ_BoardMembers_BoardId_Owner]
    ON [dbo].[BoardMembers] ([BoardId])
    WHERE [Role] = 1;
GO

CREATE INDEX [IX_BoardMembers_UserId]
    ON [dbo].[BoardMembers] ([UserId]);
GO

CREATE TABLE [dbo].[Columns]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [BoardId]            UNIQUEIDENTIFIER NOT NULL,
    [Name]               NVARCHAR(50)     NOT NULL,
    [Position]           INT              NOT NULL DEFAULT (0),
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Columns] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Columns_Boards_BoardId]
        FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id])
        ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Columns_BoardId_Position_Active]
    ON [dbo].[Columns] ([BoardId], [Position])
    WHERE [DeletedAtUtc] IS NULL;
GO

CREATE UNIQUE INDEX [UQ_Columns_BoardId_Name_Active]
    ON [dbo].[Columns] ([BoardId], [Name])
    WHERE [DeletedAtUtc] IS NULL;
GO

CREATE TABLE [dbo].[BoardTasks]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [ColumnId]           UNIQUEIDENTIFIER NOT NULL,
    [Title]              NVARCHAR(50)     NOT NULL,
    [Description]        NVARCHAR(500)    NULL,
    [Position]           INT              NOT NULL DEFAULT (0),
    [AssigneeId]         UNIQUEIDENTIFIER NULL,
    [ReporterId]         UNIQUEIDENTIFIER NOT NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_BoardTasks] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_BoardTasks_Columns_ColumnId]
        FOREIGN KEY ([ColumnId]) REFERENCES [dbo].[Columns] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_BoardTasks_Users_AssigneeId]
        FOREIGN KEY ([AssigneeId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [FK_BoardTasks_Users_ReporterId]
        FOREIGN KEY ([ReporterId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_BoardTasks_ColumnId_Position_Active]
    ON [dbo].[BoardTasks] ([ColumnId], [Position])
    WHERE [DeletedAtUtc] IS NULL;
GO
