using JustTaskTracker.Application.Users.ReadModels;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardTaskCommentReadModel(
    Guid Id,
    string Body,
    DateTime CreatedAtUtc,
    UserReadModel Author,
    DateTime? LastModifiedAtUtc = null);