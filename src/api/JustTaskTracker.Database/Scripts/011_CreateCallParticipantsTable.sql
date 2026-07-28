CREATE TABLE [dbo].[CallParticipants]
(
    [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [CallSessionId] UNIQUEIDENTIFIER NOT NULL,
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [JoinedAtUtc]   DATETIME2(7)     NOT NULL,
    [LeftAtUtc]     DATETIME2(7)     NULL,

    CONSTRAINT [PK_CallParticipants] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_CallParticipants_CallSessions_CallSessionId]
        FOREIGN KEY ([CallSessionId]) REFERENCES [dbo].[CallSessions] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_CallParticipants_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_CallParticipants_CallSessionId]
    ON [dbo].[CallParticipants] ([CallSessionId]);
GO

-- AD-12 idempotency: at most one still-active (not yet left) participant row per user per
-- session, so a concurrently-processed duplicate CallParticipantAdded delivery can't insert twice.
CREATE UNIQUE INDEX [UX_CallParticipants_ActiveParticipant]
    ON [dbo].[CallParticipants] ([CallSessionId], [UserId])
    WHERE [LeftAtUtc] IS NULL;
GO
