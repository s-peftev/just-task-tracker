using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Domain.Calls.DTOs;

public record CallSessionDto(
    Guid Id,
    Guid BoardId,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    string AcsRoomId,
    CallStatus Status,
    DateTime StartedAtUtc);
