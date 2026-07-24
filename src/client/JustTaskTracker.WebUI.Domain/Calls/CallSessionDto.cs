using JustTaskTracker.WebUI.Domain.Calls.Enums;

namespace JustTaskTracker.WebUI.Domain.Calls;

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
