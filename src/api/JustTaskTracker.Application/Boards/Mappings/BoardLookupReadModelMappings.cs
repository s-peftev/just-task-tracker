using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardLookupReadModelMappings
{
    public static BoardLookupDto ToDto(
        this BoardLookupReadModel board,
        BoardSerializationStatus boardSerializationStatus) =>
        new(
            board.Id,
            board.Name,
            board.IsArchived,
            board.UserRole,
            board.OwnerEmail,
            boardSerializationStatus,
            board.OwnerDisplayName,
            board.ArchivedAtUtc);
}
