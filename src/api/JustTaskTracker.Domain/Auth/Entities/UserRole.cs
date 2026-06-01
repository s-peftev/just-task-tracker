namespace JustTaskTracker.Domain.Auth.Entities;

public class UserRole
{
    public required Guid UserId { get; init; }
    public required int RoleId { get; init; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
