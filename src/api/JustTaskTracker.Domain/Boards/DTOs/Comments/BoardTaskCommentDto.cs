using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Boards.DTOs.Comments;

public record BoardTaskCommentDto(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    UserDto Author,
    DateTime? LastModifiedAtUtc = null);
