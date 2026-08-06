CREATE VIEW [dbo].[vw_Assistant_MyActiveBoards]
AS
SELECT
    bm.[UserId],
    b.[Id] AS [BoardId],
    b.[Name] AS [BoardName]
FROM [dbo].[BoardMembers] AS bm
INNER JOIN [dbo].[Boards] AS b ON b.[Id] = bm.[BoardId]
WHERE b.[IsDeleted] = 0
  AND b.[IsArchived] = 0;
GO
