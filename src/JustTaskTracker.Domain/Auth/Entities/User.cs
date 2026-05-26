namespace JustTaskTracker.Domain.Auth.Entities;

public class User
{
    public Guid Id { get; init; }

    public required Guid AzureAdObjectId { get; init; }

    public required string Email { get; set; }

    public string? DisplayName { get; set; }

    public required DateTime CreatedAtUtc { get; init; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
