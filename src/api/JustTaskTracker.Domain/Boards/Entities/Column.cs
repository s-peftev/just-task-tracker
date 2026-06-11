using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Boards.Entities;

public class Column : BaseEntity<Guid>, IPositionedEntity
{
    public required Guid BoardId { get; init; }
    public required string Name { get; set; }
    public int Position { get; set; }

    public Board? Board { get; set; }
    public ICollection<BoardTask> Tasks { get; set; } = [];
}
