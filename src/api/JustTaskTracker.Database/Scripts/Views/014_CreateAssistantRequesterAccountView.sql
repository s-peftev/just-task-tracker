CREATE VIEW [dbo].[vw_Assistant_RequesterAccount]
AS
SELECT
    u.[Id] AS [UserId],
    ISNULL(r.[GlobalRoles], N'') AS [GlobalRoles],
    CAST(CASE WHEN s.[Id] IS NULL THEN 0 ELSE 1 END AS BIT) AS [HasBillableSubscription],
    s.[PlanId],
    s.[Status] AS [SubscriptionStatus],
    s.[CancelAtPeriodEnd],
    s.[CurrentPeriodStartUtc],
    s.[CurrentPeriodEndUtc]
FROM [dbo].[Users] AS u
LEFT JOIN (
    SELECT
        [UserId],
        STRING_AGG([Role], N',') WITHIN GROUP (ORDER BY [Role]) AS [GlobalRoles]
    FROM [dbo].[UserGlobalRoles]
    GROUP BY [UserId]
) AS r ON r.[UserId] = u.[Id]
LEFT JOIN [dbo].[Subscriptions] AS s
    ON s.[UserId] = u.[Id]
   AND s.[Status] IN (N'active', N'trialing', N'past_due')
WHERE u.[IsDeleted] = 0;
GO
