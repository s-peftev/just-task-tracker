using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common;

namespace JustTaskTracker.Domain.Kanban.Entities;

public class BoardTask : BaseEntity<Guid>
{
    public required Guid ColumnId { get; init; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int Position { get; set; }
    public Guid? AssigneeId { get; set; }
    public required Guid ReporterId { get; init; }

    public Column Column { get; set; } = null!;
    public User? Assignee { get; set; }
    public User Reporter { get; set; } = null!;
}
