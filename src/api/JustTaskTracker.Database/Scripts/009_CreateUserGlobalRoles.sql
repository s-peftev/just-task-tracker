CREATE TABLE [dbo].[UserGlobalRoles]
(
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Role]   NVARCHAR(16)     NOT NULL,

    CONSTRAINT [PK_UserGlobalRoles] PRIMARY KEY CLUSTERED ([UserId], [Role]),
    CONSTRAINT [FK_UserGlobalRoles_Users_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
        ON DELETE CASCADE,
    CONSTRAINT [CK_UserGlobalRoles_Role] CHECK (
        [Role] IN (N'ADMIN', N'USER', N'GUEST'))
);
GO

INSERT INTO [dbo].[UserGlobalRoles] ([UserId], [Role])
SELECT [Id], N'GUEST'
FROM [dbo].[Users];
GO
