namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportBoardDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    bool IsArchived,
    DateTime? ArchivedAtUtc,
    int ColumnCount,
    int TaskCount);
