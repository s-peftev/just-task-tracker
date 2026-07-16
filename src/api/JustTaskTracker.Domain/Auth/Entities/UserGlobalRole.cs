namespace JustTaskTracker.Domain.Auth.Entities;

/// <summary>
/// A global Azure AD app role assigned to a user.
/// A user may have multiple roles.
/// </summary>
public class UserGlobalRole
{
    public required Guid UserId { get; init; }
    public required string Role { get; set; }

    public User? User { get; set; }
}
