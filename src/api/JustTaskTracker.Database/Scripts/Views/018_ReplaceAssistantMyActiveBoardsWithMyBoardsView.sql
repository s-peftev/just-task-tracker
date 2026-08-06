IF OBJECT_ID(N'[dbo].[vw_Assistant_MyActiveBoards]', N'V') IS NOT NULL
    DROP VIEW [dbo].[vw_Assistant_MyActiveBoards];
GO

CREATE VIEW [dbo].[vw_Assistant_MyBoards]
AS
SELECT
    bm.[UserId],
    b.[Id] AS [BoardId],
    b.[Name] AS [BoardName],
    b.[IsArchived]
FROM [dbo].[BoardMembers] AS bm
INNER JOIN [dbo].[Boards] AS b ON b.[Id] = bm.[BoardId]
WHERE b.[IsDeleted] = 0;
GO
