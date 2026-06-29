using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.DTOs.Boards;

public record BoardArchivedDto(
    DateTime ArchivedAtUtc,
    BoardSerializationStatus BoardSerializationStatus);
