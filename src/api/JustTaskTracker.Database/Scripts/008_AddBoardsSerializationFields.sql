ALTER TABLE [dbo].[Boards]
ADD
    [IsSerialized]       BIT              NOT NULL DEFAULT (0),
    [SerializedAtUtc]    DATETIME2(7)     NULL;
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_Serialize] CHECK (
    ([IsSerialized] = 0 AND [SerializedAtUtc] IS NULL) OR
    ([IsSerialized] = 1 AND [SerializedAtUtc] IS NOT NULL));
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_SerializedRequiresArchive] CHECK (
    [IsSerialized] = 0 OR [IsArchived] = 1);
GO
