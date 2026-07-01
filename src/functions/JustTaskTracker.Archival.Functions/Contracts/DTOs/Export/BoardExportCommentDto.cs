namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public sealed record BoardExportCommentDto(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    BoardExportUserDto Author);
