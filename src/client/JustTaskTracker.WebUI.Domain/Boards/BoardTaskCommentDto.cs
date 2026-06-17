using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskCommentDto(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    UserDto Author,
    DateTime? LastModifiedAtUtc = null);
