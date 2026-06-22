namespace JustTaskTracker.Domain.Auth.DTOs;

public record UserWithRolesDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    string? DisplayName,
    string? ProfilePhotoUrl);
