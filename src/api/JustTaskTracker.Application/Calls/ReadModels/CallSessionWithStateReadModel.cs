using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Application.Calls.ReadModels;

// One row per active CallSession, with its live state (participants/allowed users/linked tasks/
// creator) projected in the same query via CallSession's navigation properties (AD-2/AD-10 live
// state), instead of one round trip per session.
public record CallSessionWithStateReadModel(
    Guid Id,
    Guid BoardId,
    UserReadModel CreatedBy,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    string AcsRoomId,
    CallStatus Status,
    DateTime StartedAtUtc,
    IReadOnlyList<UserReadModel> AllowedUsers,
    IReadOnlyList<BoardTaskLookupDto> LinkedTasks,
    IReadOnlyList<UserReadModel> Participants,
    Guid? CurrentPresenterUserId);
