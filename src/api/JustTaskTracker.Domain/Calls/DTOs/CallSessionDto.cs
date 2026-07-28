using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Domain.Calls.DTOs;

public record CallSessionDto(
    Guid Id,
    Guid BoardId,
    UserDto CreatedBy,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    string AcsRoomId,
    CallStatus Status,
    DateTime StartedAtUtc,
    IReadOnlyList<UserDto>? AllowedUsers,
    IReadOnlyList<BoardTaskLookupDto> LinkedTasks,
    IReadOnlyList<UserDto> Participants,
    Guid? CurrentPresenterUserId);
