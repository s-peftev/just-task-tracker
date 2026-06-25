using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardTaskDetailsReadModel(
    Guid Id,
    Guid ColumnId,
    string ColumnName,
    string Title,
    int Position,
    DateTime CreatedAtUtc,
    UserReadModel Reporter,
    BoardMemberRole UserRole,
    IReadOnlyList<BoardTaskAttachmentReadModel> Attachments,
    string? Description = null,
    UserReadModel? Assignee = null);
