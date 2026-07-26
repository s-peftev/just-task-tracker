namespace JustTaskTracker.WebUI.Domain.Calls;

public record JoinCallResponse(string AcsRoomId, string Token, DateTimeOffset ExpiresOn);
