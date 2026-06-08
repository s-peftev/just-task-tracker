using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Domain.Boards.Entities;

public class BoardMember
{
    public required Guid BoardId { get; init; }
    public required Guid UserId { get; init; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public BoardMemberRole Role { get; set; }

    public Board Board { get; set; } = null!;
    public User User { get; set; } = null!;
}
