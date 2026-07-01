namespace JustTaskTracker.Archival.Functions.Contracts.DTOs.Export;

public record BoardExportTaskDto(
    Guid Id,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    BoardExportUserDto Reporter,
    BoardExportUserDto? Assignee,
    string? Description,
    IReadOnlyList<BoardExportCommentDto>? Comments,
    IReadOnlyList<BoardExportAttachmentDto>? Attachments);
