ALTER TABLE [dbo].[Boards] DROP CONSTRAINT [CK_Boards_ReExportRequiresSerialization];
GO

ALTER TABLE [dbo].[Boards] DROP CONSTRAINT [CK_Boards_SerializedRequiresArchive];
GO

ALTER TABLE [dbo].[Boards] DROP CONSTRAINT [CK_Boards_Serialize];
GO

EXEC sp_rename N'dbo.Boards.IsSerialized', N'IsExported', N'COLUMN';
GO

EXEC sp_rename N'dbo.Boards.SerializedAtUtc', N'ExportedAtUtc', N'COLUMN';
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_Export] CHECK (
    ([IsExported] = 0 AND [ExportedAtUtc] IS NULL) OR
    ([IsExported] = 1 AND [ExportedAtUtc] IS NOT NULL));
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_ExportedRequiresArchive] CHECK (
    [IsExported] = 0 OR [IsArchived] = 1);
GO

ALTER TABLE [dbo].[Boards]
ADD CONSTRAINT [CK_Boards_ReExportRequiresExport] CHECK (
    [IsReExportRequested] = 0 OR [IsExported] = 1);
GO
