namespace JustTaskTracker.Domain.Auth.DTOs;

public record UserWithRolesDto(
    Guid Id,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles);
