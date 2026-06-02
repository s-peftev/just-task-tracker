using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Services.Abstractions.Auth;

public interface IAuthApiService
{
    Task<UserWithRolesDto> LoginAsync(CancellationToken ct = default);
    Task<UserWithRolesDto?> GetCurrentUserAsync(CancellationToken ct = default);
}
