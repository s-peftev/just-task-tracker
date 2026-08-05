CREATE VIEW [dbo].[vw_Assistant_AssignedTasks]
AS
SELECT
    bm.[UserId],
    b.[Id] AS [BoardId],
    t.[Id] AS [TaskId],
    t.[Title],
    t.[CreatedAtUtc]
FROM [dbo].[BoardMembers] AS bm
INNER JOIN [dbo].[Boards] AS b ON b.[Id] = bm.[BoardId]
INNER JOIN [dbo].[Columns] AS c ON c.[BoardId] = b.[Id]
INNER JOIN [dbo].[BoardTasks] AS t ON t.[ColumnId] = c.[Id]
WHERE b.[IsDeleted] = 0
  AND b.[IsArchived] = 0
  AND c.[IsDeleted] = 0
  AND t.[IsDeleted] = 0
  AND t.[AssigneeId] = bm.[UserId];
GO
