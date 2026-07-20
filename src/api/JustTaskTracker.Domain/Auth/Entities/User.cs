using JustTaskTracker.Domain.Common.Entities;

namespace JustTaskTracker.Domain.Auth.Entities;

public class User : BaseEntity<Guid>
{
    public required Guid AzureAdObjectId { get; init; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePhotoVersion { get; set; }

    public ICollection<UserGlobalRole> GlobalRoles { get; set; } = [];
}
