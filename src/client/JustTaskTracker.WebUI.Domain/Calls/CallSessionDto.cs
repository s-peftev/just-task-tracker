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
    DateTime StartedAtUtc,
    // Null for Open sessions; for Restricted sessions, lets the client locally decide whether to
    // show the Join action as enabled for the current user (AD-4) without a round trip.
    IReadOnlyList<Guid>? AllowedUserIds);
