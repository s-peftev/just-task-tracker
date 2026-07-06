using JustTaskTracker.Application.Auth;
using System.Security.Claims;

namespace JustTaskTracker.Infrastructure.Auth;

internal sealed class CurrentUserContext : ICurrentUserContext
{
    public ClaimsPrincipal? User { get; set; }
}
