using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Domain.Kanban.Entities;

public class Board : BaseEntity<Guid>
{
    public required string Name { get; set; }

    public ICollection<BoardMember> Members { get; set; } = [];
    public ICollection<Column> Columns { get; set; } = [];
}
