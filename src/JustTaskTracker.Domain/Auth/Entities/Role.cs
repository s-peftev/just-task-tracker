namespace JustTaskTracker.Domain.Auth.Entities;

public class Role
{
    public int Id { get; init; }

    public required string Name { get; set; }

    public required string NormalizedName { get; set; }

    public string? Description { get; set; }

    public required DateTime CreatedAtUtc { get; init; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
