namespace JustTaskTracker.WebUI.Domain.Auth;

public record UserWithRolesDto(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    string? DisplayName);
