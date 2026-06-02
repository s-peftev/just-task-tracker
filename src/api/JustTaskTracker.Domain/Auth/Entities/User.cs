using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Domain.Auth.Entities;

public class User : BaseEntity<Guid>
{
    public required Guid AzureAdObjectId { get; init; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
}
