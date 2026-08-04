CREATE VIEW [dbo].[vw_Assistant_ActiveOwnedBoardsCount]
AS
SELECT
    bm.[UserId],
    COUNT_BIG(*) AS [ActiveOwnedBoardsCount]
FROM [dbo].[BoardMembers] AS bm
INNER JOIN [dbo].[Boards] AS b ON b.[Id] = bm.[BoardId]
WHERE bm.[Role] = 1
  AND b.[IsArchived] = 0
  AND b.[IsDeleted] = 0
GROUP BY bm.[UserId];
GO
