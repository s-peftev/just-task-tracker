CREATE TABLE [dbo].[CallSessionAllowedParticipants]
(
    [CallSessionId] UNIQUEIDENTIFIER NOT NULL,
    [UserId]        UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT [PK_CallSessionAllowedParticipants] PRIMARY KEY CLUSTERED ([CallSessionId], [UserId]),
    CONSTRAINT [FK_CallSessionAllowedParticipants_CallSessions_CallSessionId]
        FOREIGN KEY ([CallSessionId]) REFERENCES [dbo].[CallSessions] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_CallSessionAllowedParticipants_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO
