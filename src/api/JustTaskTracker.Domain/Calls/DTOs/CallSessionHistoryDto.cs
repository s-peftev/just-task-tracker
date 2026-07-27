using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Domain.Calls.DTOs;

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
