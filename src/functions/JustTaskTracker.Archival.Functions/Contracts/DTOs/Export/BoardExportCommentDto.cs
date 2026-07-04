namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportCommentDto(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    BoardExportUserDto Author);
