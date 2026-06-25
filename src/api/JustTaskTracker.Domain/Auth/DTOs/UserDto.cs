namespace JustTaskTracker.Domain.Auth.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    string? ProfilePhotoUrl);
