using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Domain.Boards;
using JustTaskTracker.WebUI.Domain.Calls.Enums;

namespace JustTaskTracker.WebUI.Domain.Calls;

public record CallSessionHistoryDto(
    Guid Id,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    UserDto CreatedBy,
    DateTime StartedAtUtc,
    DateTime EndedAtUtc,
    IReadOnlyList<BoardTaskLookupDto> LinkedTasks,
    IReadOnlyList<CallHistoryParticipantDto> Participants,
    IReadOnlyList<UserDto>? AllowedUsers);
