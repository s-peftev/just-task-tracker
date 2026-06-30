using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardDetailsReadModelMappings
{
    public static BoardDetailsDto ToDto(
        this BoardDetailsReadModel board,
        BoardExportStatusInfo? exportInfo)
    {
        var boardExportStatus = board.IsArchived
            ? exportInfo?.Status ?? BoardExportStatus.None
            : BoardExportStatus.None;

        var exportOptions = board.IsArchived
            ? exportInfo?.ExportOptions
            : null;

        return new BoardDetailsDto(
            board.Id,
            board.Name,
            board.CreatedAtUtc,
            board.IsArchived,
            board.UserRole,
            board.Columns,
            boardExportStatus,
            exportOptions,
            board.ArchivedAtUtc);
    }
}
