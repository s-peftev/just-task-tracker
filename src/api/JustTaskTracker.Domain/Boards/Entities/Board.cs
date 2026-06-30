using JustTaskTracker.Domain.Common.Entities;

namespace JustTaskTracker.Domain.Boards.Entities;

public class Board : BaseEntity<Guid>
{
    public required string Name { get; set; }
    public bool IsArchived { get; set; }
    public bool IsExported { get; set; }
    public bool IsReExportRequested { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    public DateTime? ExportedAtUtc { get; set; }

    public ICollection<BoardMember> Members { get; set; } = [];
    public ICollection<Column> Columns { get; set; } = [];
}
