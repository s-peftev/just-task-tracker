using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardLookupReadModelMappings
{
    public static BoardLookupDto ToDto(
        this BoardLookupReadModel board,
        BoardExportStatusInfo? exportInfo)
    {
        var boardExportStatus = board.IsArchived
            ? exportInfo?.ExportStatus ?? BoardExportStatus.None
            : BoardExportStatus.None;

        var reExportStatus = board.IsArchived
            ? exportInfo?.ReExportStatus ?? BoardExportStatus.None
            : BoardExportStatus.None;

        return new BoardLookupDto(
            board.Id,
            board.Name,
            board.IsArchived,
            board.UserRole,
            board.OwnerEmail,
            boardExportStatus,
            reExportStatus,
            board.OwnerDisplayName,
            board.ArchivedAtUtc);
    }
}
