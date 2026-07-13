CREATE TABLE [dbo].[Subscriptions]
(
    [Id]                     UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [UserId]                 UNIQUEIDENTIFIER NOT NULL,
    [PlanId]                 NVARCHAR(64)     NOT NULL,
    [StripeCustomerId]       NVARCHAR(255)    NOT NULL,
    [StripeSubscriptionId]   NVARCHAR(255)    NOT NULL,
    [Status]                 NVARCHAR(32)     NOT NULL,
    [CurrentPeriodStartUtc]  DATETIME2(7)     NULL,
    [CurrentPeriodEndUtc]    DATETIME2(7)     NULL,
    [CancelAtPeriodEnd]      BIT              NOT NULL DEFAULT (0),
    [CreatedAtUtc]           DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]              NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]      DATETIME2(7)     NULL,
    [LastModifiedBy]         NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Subscriptions_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [UQ_Subscriptions_StripeSubscriptionId]
    ON [dbo].[Subscriptions] ([StripeSubscriptionId]);
GO

CREATE UNIQUE INDEX [UQ_Subscriptions_UserId_Billable]
    ON [dbo].[Subscriptions] ([UserId])
    WHERE [Status] IN (N'active', N'trialing', N'past_due');
GO

CREATE INDEX [IX_Subscriptions_UserId]
    ON [dbo].[Subscriptions] ([UserId]);
GO

CREATE INDEX [IX_Subscriptions_StripeCustomerId]
    ON [dbo].[Subscriptions] ([StripeCustomerId]);
GO

CREATE TABLE [dbo].[StripeWebhookEvents]
(
    [EventId]              NVARCHAR(255)    NOT NULL,
    [EventType]            NVARCHAR(128)    NOT NULL,
    [ReceivedAtUtc]        DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [ProcessedAtUtc]       DATETIME2(7)     NULL,
    [LastError]            NVARCHAR(2000)   NULL,

    CONSTRAINT [PK_StripeWebhookEvents] PRIMARY KEY CLUSTERED ([EventId])
);
GO

CREATE INDEX [IX_StripeWebhookEvents_Unprocessed]
    ON [dbo].[StripeWebhookEvents] ([ReceivedAtUtc])
    WHERE [ProcessedAtUtc] IS NULL;
GO
