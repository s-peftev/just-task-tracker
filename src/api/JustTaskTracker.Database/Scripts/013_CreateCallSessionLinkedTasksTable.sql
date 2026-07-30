CREATE TABLE [dbo].[CallSessionLinkedTasks]
(
    [CallSessionId] UNIQUEIDENTIFIER NOT NULL,
    [TaskId]        UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT [PK_CallSessionLinkedTasks] PRIMARY KEY CLUSTERED ([CallSessionId], [TaskId]),
    CONSTRAINT [FK_CallSessionLinkedTasks_CallSessions_CallSessionId]
        FOREIGN KEY ([CallSessionId]) REFERENCES [dbo].[CallSessions] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [FK_CallSessionLinkedTasks_BoardTasks_TaskId]
        FOREIGN KEY ([TaskId]) REFERENCES [dbo].[BoardTasks] ([Id])
        ON DELETE NO ACTION
);
GO
