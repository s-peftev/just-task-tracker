namespace JustTaskTracker.WebUI.Domain.Calls;

public record CallParticipantDto(
    Guid UserId,
    string AcsCommunicationUserId,
    string? DisplayName,
    string Email,
    string? ProfilePhotoUrl,
    DateTime JoinedAtUtc);
