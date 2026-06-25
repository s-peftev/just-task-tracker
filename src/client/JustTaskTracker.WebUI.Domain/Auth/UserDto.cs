namespace JustTaskTracker.WebUI.Domain.Auth;

public record UserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? ProfilePhotoUrl);
