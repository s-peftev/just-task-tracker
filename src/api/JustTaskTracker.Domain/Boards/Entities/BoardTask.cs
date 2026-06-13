using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Boards.Entities;

public class BoardTask : BaseEntity<Guid>, IPositionedEntity
{
    public required Guid ColumnId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public Guid? AssigneeId { get; set; }
    public required Guid ReporterId { get; init; }

    public Column? Column { get; set; }
    public User? Assignee { get; set; }
    public User? Reporter { get; set; }
    public ICollection<BoardTaskComment> Comments { get; set; } = [];
}
