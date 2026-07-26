CREATE TABLE [dbo].[CallSessions]
(
    [Id]                     UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [BoardId]                UNIQUEIDENTIFIER NOT NULL,
    [CreatedByUserId]        UNIQUEIDENTIFIER NOT NULL,
    [Title]                  NVARCHAR(50)     NOT NULL,
    [Topic]                  NVARCHAR(200)    NULL,
    [Visibility]             TINYINT          NOT NULL, -- 1 = Open, 2 = Restricted
    [AcsRoomId]              NVARCHAR(255)    NOT NULL,
    [Status]                 TINYINT          NOT NULL, -- 1 = Active, 2 = Closed
    [CurrentPresenterUserId] UNIQUEIDENTIFIER NULL,
    [StartedAtUtc]           DATETIME2(7)     NOT NULL,
    [EndedAtUtc]             DATETIME2(7)     NULL,
    [CreatedAtUtc]           DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]              NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]      DATETIME2(7)     NULL,
    [LastModifiedBy]         NVARCHAR(450)    NULL,
    [IsDeleted]              BIT              NOT NULL DEFAULT (0),
    [DeletedAtUtc]           DATETIME2(7)     NULL,
    [DeletedBy]              NVARCHAR(450)    NULL,

    CONSTRAINT [PK_CallSessions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_CallSessions_Boards_BoardId]
        FOREIGN KEY ([BoardId]) REFERENCES [dbo].[Boards] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_CallSessions_Users_CreatedByUserId]
        FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [FK_CallSessions_Users_CurrentPresenterUserId]
        FOREIGN KEY ([CurrentPresenterUserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION,
    CONSTRAINT [CK_CallSessions_SoftDelete] CHECK (
        ([IsDeleted] = 0 AND [DeletedAtUtc] IS NULL) OR
        ([IsDeleted] = 1 AND [DeletedAtUtc] IS NOT NULL))
);
GO

CREATE INDEX [IX_CallSessions_BoardId]
    ON [dbo].[CallSessions] ([BoardId]);
GO

CREATE TABLE [dbo].[AcsUserIdentityMappings]
(
    [UserId]                 UNIQUEIDENTIFIER NOT NULL,
    [AcsCommunicationUserId] NVARCHAR(255)    NOT NULL,

    CONSTRAINT [PK_AcsUserIdentityMappings] PRIMARY KEY CLUSTERED ([UserId]),
    CONSTRAINT [FK_AcsUserIdentityMappings_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO
