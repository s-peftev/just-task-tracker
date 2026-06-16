namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskPreviewDto(
    Guid Id,
    string Title,
    int Position);
