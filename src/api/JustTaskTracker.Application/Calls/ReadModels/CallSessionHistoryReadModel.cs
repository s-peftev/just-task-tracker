using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Application.Calls.ReadModels;

// One row per closed CallSession, with its history state (linked tasks/participant events/
// allowed users/creator) projected in the same query via CallSession's navigation properties.
public record CallSessionHistoryReadModel(
    Guid Id,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    UserReadModel CreatedBy,
    DateTime StartedAtUtc,
    DateTime EndedAtUtc,
    IReadOnlyList<BoardTaskLookupDto> LinkedTasks,
    IReadOnlyList<CallParticipantEventReadModel> ParticipantEvents,
    IReadOnlyList<UserReadModel> AllowedUsers);
