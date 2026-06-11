namespace JustTaskTracker.WebUI.Domain.Boards;

public record TaskLookupDto(
    Guid Id,
    string Title,
    int Position);
