namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record UpdateBoardTaskRequest(
    string? Title = null,
    string? Description = null,
    Guid? AssigneeId = null);
