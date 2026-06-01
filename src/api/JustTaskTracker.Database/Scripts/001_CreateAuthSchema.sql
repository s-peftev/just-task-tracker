/*
    Auth schema for Azure Entra ID (Microsoft Entra) integration.
    Maps Entra users (oid claim) to application roles stored in the database.
*/

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'auth') EXEC(N'CREATE SCHEMA [auth]');
GO

CREATE TABLE [auth].[Roles]
(
    [Id]                 INT           NOT NULL IDENTITY(1, 1),
    [Name]               NVARCHAR(64)  NOT NULL,
    [NormalizedName]     NVARCHAR(64)  NOT NULL,
    [Description]        NVARCHAR(256) NULL,
    [CreatedAtUtc]       DATETIME2(7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450) NULL,
    [LastModifiedAtUtc]  DATETIME2(7)  NULL,
    [LastModifiedBy]     NVARCHAR(450) NULL,
    [DeletedAtUtc]       DATETIME2(7)  NULL,
    [DeletedBy]          NVARCHAR(450) NULL,

    CONSTRAINT [PK_auth_Roles] PRIMARY KEY CLUSTERED ([Id])
);
GO

CREATE UNIQUE INDEX [UQ_auth_Roles_NormalizedName] ON [auth].[Roles] ([NormalizedName]);
GO

CREATE TABLE [auth].[Users]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [AzureAdObjectId]    UNIQUEIDENTIFIER NOT NULL,
    [Email]              NVARCHAR(320)    NOT NULL,
    [DisplayName]        NVARCHAR(256)    NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_auth_Users] PRIMARY KEY CLUSTERED ([Id])
);
GO

CREATE UNIQUE INDEX [UQ_auth_Users_AzureAdObjectId] ON [auth].[Users] ([AzureAdObjectId]);
GO

CREATE UNIQUE INDEX [UQ_auth_Users_Email] ON [auth].[Users] ([Email]);
GO

CREATE TABLE [auth].[UserRoles]
(
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] INT              NOT NULL,

    CONSTRAINT [PK_auth_UserRoles] PRIMARY KEY CLUSTERED ([UserId], [RoleId]),
    CONSTRAINT [FK_auth_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [auth].[Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_auth_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [auth].[Roles] ([Id])
);
GO

CREATE INDEX [IX_auth_UserRoles_RoleId] ON [auth].[UserRoles] ([RoleId]);
GO