using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Domain.Calls;

public record CallHistoryParticipantDto(
    UserDto User,
    DateTime FirstJoinedAtUtc,
    DateTime? LastLeftAtUtc);
