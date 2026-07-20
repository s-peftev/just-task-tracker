using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardMemberReadModel(
    UserReadModel User,
    IReadOnlyList<string> GlobalRoles,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
