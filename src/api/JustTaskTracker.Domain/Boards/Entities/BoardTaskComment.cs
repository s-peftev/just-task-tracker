using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common.Entities;

namespace JustTaskTracker.Domain.Boards.Entities;

public class BoardTaskComment : BaseEntity<Guid>
{
    public required Guid BoardTaskId { get; init; }
    public required Guid AuthorId { get; init; }
    public required string Body { get; set; }

    public BoardTask? BoardTask { get; set; }
    public User? Author { get; set; }
}
