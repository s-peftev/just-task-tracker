using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Domain.Kanban.Entities;

public class Column : BaseEntity<Guid>
{
    public required Guid BoardId { get; init; }
    public required string Name { get; set; }
    public int Position { get; set; }

    public Board Board { get; set; } = null!;
    public ICollection<BoardTask> Tasks { get; set; } = [];
}
