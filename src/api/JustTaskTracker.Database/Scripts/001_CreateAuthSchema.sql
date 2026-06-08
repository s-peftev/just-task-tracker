CREATE TABLE [dbo].[Users]
(
    [Id]                 UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()),
    [AzureAdObjectId]    UNIQUEIDENTIFIER NOT NULL,
    [Email]              NVARCHAR(320)    NOT NULL,
    [DisplayName]        NVARCHAR(256)    NULL,
    [CreatedAtUtc]       DATETIME2(7)     NOT NULL DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]          NVARCHAR(450)    NULL,
    [LastModifiedAtUtc]  DATETIME2(7)     NULL,
    [LastModifiedBy]     NVARCHAR(450)    NULL,
    [IsDeleted]          BIT              NOT NULL DEFAULT (0),
    [DeletedAtUtc]       DATETIME2(7)     NULL,
    [DeletedBy]          NVARCHAR(450)    NULL,

    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [CK_Users_SoftDelete] CHECK (
        ([IsDeleted] = 0 AND [DeletedAtUtc] IS NULL) OR
        ([IsDeleted] = 1 AND [DeletedAtUtc] IS NOT NULL))
);
GO

CREATE UNIQUE INDEX [UQ_Users_AzureAdObjectId_Active] 
    ON [dbo].[Users] ([AzureAdObjectId])
    WHERE [IsDeleted] = 0;
GO

CREATE UNIQUE INDEX [UQ_Users_Email_Active] 
    ON [dbo].[Users] ([Email])
    WHERE [IsDeleted] = 0;
GO
