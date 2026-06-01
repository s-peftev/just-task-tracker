using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Domain.Auth.Entities;

public class Role : BaseEntity<int>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
