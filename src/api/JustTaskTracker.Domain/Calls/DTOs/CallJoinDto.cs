namespace JustTaskTracker.Domain.Calls.DTOs;

public record CallJoinDto(string AcsRoomId, string Token, DateTimeOffset ExpiresOn);
