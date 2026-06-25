using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardMemberReadModel(
    UserReadModel User,
    BoardMemberRole Role,
    DateTime JoinedAtUtc);
