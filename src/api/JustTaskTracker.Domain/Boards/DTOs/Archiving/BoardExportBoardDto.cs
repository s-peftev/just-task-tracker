namespace JustTaskTracker.Domain.Boards.DTOs.Archiving;

public record BoardExportBoardDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    bool IsArchived,
    DateTime? ArchivedAtUtc,
    int ColumnCount,
    int TaskCount);
