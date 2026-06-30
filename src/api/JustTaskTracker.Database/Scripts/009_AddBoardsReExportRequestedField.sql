ALTER TABLE [dbo].[Boards]
ADD [IsReExportRequested] BIT NOT NULL DEFAULT (0);
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_ReExportRequiresSerialization] CHECK (
    [IsReExportRequested] = 0 OR [IsSerialized] = 1);
GO
