namespace JustTaskTracker.Domain.Boards.DTOs.BoardTasks;

public record BoardTaskPreviewDto(
    Guid Id,
    string Title,
    int Position,
    int CommentsCount,
    int AttachmentsCount);
