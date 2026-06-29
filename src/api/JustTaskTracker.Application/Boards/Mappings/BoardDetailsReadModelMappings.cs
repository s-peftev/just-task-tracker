using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardDetailsReadModelMappings
{
    public static BoardDetailsDto ToDto(
        this BoardDetailsReadModel board,
        BoardSerializationStatusInfo? serializationInfo)
    {
        var boardSerializationStatus = board.IsArchived
            ? serializationInfo?.Status ?? BoardSerializationStatus.None
            : BoardSerializationStatus.None;

        var exportOptions = board.IsArchived
            ? serializationInfo?.ExportOptions
            : null;

        return new BoardDetailsDto(
            board.Id,
            board.Name,
            board.CreatedAtUtc,
            board.IsArchived,
            board.UserRole,
            board.Columns,
            boardSerializationStatus,
            exportOptions,
            board.ArchivedAtUtc);
    }
}
