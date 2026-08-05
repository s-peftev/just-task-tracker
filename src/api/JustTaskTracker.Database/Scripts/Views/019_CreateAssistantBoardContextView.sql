CREATE VIEW [dbo].[vw_Assistant_BoardContext]
AS
SELECT
    bm.[UserId],
    b.[Id] AS [BoardId],
    b.[Name] AS [BoardName],
    b.[IsArchived],
    bm.[Role] AS [MemberRole],
    ownerBm.[UserId] AS [OwnerUserId],
    (
        SELECT COUNT(*)
        FROM [dbo].[Columns] AS c
        WHERE c.[BoardId] = b.[Id]
          AND c.[IsDeleted] = 0
    ) AS [ColumnCount],
    (
        SELECT COUNT(*)
        FROM [dbo].[BoardTasks] AS t
        INNER JOIN [dbo].[Columns] AS c ON c.[Id] = t.[ColumnId]
        WHERE c.[BoardId] = b.[Id]
          AND c.[IsDeleted] = 0
          AND t.[IsDeleted] = 0
    ) AS [TaskCount],
    (
        SELECT COUNT(*)
        FROM [dbo].[BoardMembers] AS m
        WHERE m.[BoardId] = b.[Id]
    ) AS [MemberCount]
FROM [dbo].[BoardMembers] AS bm
INNER JOIN [dbo].[Boards] AS b ON b.[Id] = bm.[BoardId]
INNER JOIN [dbo].[BoardMembers] AS ownerBm
    ON ownerBm.[BoardId] = b.[Id]
   AND ownerBm.[Role] = 1
WHERE b.[IsDeleted] = 0;
GO
