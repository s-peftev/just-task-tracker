using JustTaskTracker.Domain.Boards.DTOs.Columns;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardDetailsReadModel(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    bool IsArchived,
    BoardMemberRole UserRole,
    IEnumerable<ColumnDto> Columns,
    Guid? OwnerUserId,
    DateTime? ArchivedAtUtc);
