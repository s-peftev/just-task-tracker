ALTER TABLE [dbo].[Boards]
ADD
    [IsArchived]         BIT              NOT NULL DEFAULT (0),
    [ArchivedAtUtc]      DATETIME2(7)     NULL;
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_Archive] CHECK (
    ([IsArchived] = 0 AND [ArchivedAtUtc] IS NULL) OR
    ([IsArchived] = 1 AND [ArchivedAtUtc] IS NOT NULL));
GO
