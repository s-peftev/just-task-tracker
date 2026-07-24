using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Domain.Calls.DTOs;

public record CallSessionDto(
    Guid Id,
    Guid BoardId,
    Guid CreatedByUserId,
    string Title,
    string? Topic,
    CallVisibility Visibility,
    string AcsRoomId,
    CallStatus Status,
    DateTime StartedAtUtc);
