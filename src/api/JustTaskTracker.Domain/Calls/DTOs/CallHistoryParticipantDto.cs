using JustTaskTracker.Domain.Auth.DTOs;

namespace JustTaskTracker.Domain.Calls.DTOs;

public record CallHistoryParticipantDto(
    UserDto User,
    DateTime FirstJoinedAtUtc,
    DateTime? LastLeftAtUtc);
