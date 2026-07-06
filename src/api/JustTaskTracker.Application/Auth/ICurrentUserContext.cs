using System.Security.Claims;

namespace JustTaskTracker.Application.Auth;

/// <summary>
/// Scoped holder for the current user's claims principal when it is not available
/// via <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/> (e.g. SignalR hub invocations).
/// </summary>
public interface ICurrentUserContext
{
    ClaimsPrincipal? User { get; set; }
}
