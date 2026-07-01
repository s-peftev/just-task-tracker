namespace JustTaskTracker.Domain.Boards.DTOs.Archiving;

public record BoardExportCommentDto(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    BoardExportUserDto Author);
