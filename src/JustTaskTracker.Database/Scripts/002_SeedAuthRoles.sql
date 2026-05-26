/*
    Reference data for application roles (idempotent MERGE).
*/

SET IDENTITY_INSERT [auth].[Roles] ON;

MERGE [auth].[Roles] AS [target]
USING (VALUES
    (1, N'Admin', N'ADMIN', N'Full administrative access to the application.'),
    (2, N'User',  N'USER',  N'Standard user access to boards and tasks.')
) AS [source] ([Id], [Name], [NormalizedName], [Description])
    ON [target].[NormalizedName] = [source].[NormalizedName]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [NormalizedName], [Description])
    VALUES ([source].[Id], [source].[Name], [source].[NormalizedName], [source].[Description])
WHEN MATCHED AND (
    [target].[Name]        <> [source].[Name]
    OR ISNULL([target].[Description], N'') <> ISNULL([source].[Description], N'')
) THEN
    UPDATE SET
        [Name]        = [source].[Name],
        [Description] = [source].[Description];

SET IDENTITY_INSERT [auth].[Roles] OFF;
