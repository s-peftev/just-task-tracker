using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardDetailsReadModelMappings
{
    public static BoardDetailsDto ToDto(
        this BoardDetailsReadModel board,
        BoardSerializationStatus boardSerializationStatus) =>
        new(
            board.Id,
            board.Name,
            board.CreatedAtUtc,
            board.IsArchived,
            board.UserRole,
            board.Columns,
            boardSerializationStatus,
            board.ArchivedAtUtc);
}
