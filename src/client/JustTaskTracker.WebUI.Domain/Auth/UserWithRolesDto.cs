namespace JustTaskTracker.WebUI.Domain.Auth;

public record UserWithRolesDto(
    Guid Id,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles);
