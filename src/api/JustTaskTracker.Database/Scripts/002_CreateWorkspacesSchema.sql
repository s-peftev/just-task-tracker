CREATE TABLE [dbo].[Workspaces]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [OwnerId]            UNIQUEIDENTIFIER NOT NULL,
    [Name]               NVARCHAR(50)     NOT NULL,
    [ShortName]          NVARCHAR(5)      NOT NULL,
    [Description]        NVARCHAR(500)    NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Workspaces] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Workspaces_Users_OwnerId]
        FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE INDEX [IX_Workspaces_OwnerId_Active]
    ON [dbo].[Workspaces] ([OwnerId])
    WHERE [DeletedAtUtc] IS NULL;
GO

CREATE UNIQUE INDEX [UQ_Workspaces_OwnerId_ShortName_Active]
    ON [dbo].[Workspaces] ([OwnerId], [ShortName])
    WHERE [DeletedAtUtc] IS NULL;
GO

CREATE TABLE [dbo].[WorkspaceMembers]
(
    [WorkspaceId]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]             UNIQUEIDENTIFIER NOT NULL,
    [JoinedAtUtc]        DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [Role]               NVARCHAR(10)     NOT NULL,

    CONSTRAINT [PK_WorkspaceMembers] PRIMARY KEY CLUSTERED ([WorkspaceId], [UserId]),
    CONSTRAINT [FK_WorkspaceMembers_Workspaces_WorkspaceId]
        FOREIGN KEY ([WorkspaceId]) REFERENCES [dbo].[Workspaces] ([Id]),
    CONSTRAINT [FK_WorkspaceMembers_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);
GO

CREATE INDEX [IX_WorkspaceMembers_UserId]
    ON [dbo].[WorkspaceMembers] ([UserId]);
GO
