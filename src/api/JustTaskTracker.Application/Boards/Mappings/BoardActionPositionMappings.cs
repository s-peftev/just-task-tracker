using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Boards.Notifications.BoardActions.Payloads.Positions;

namespace JustTaskTracker.Application.Boards.Mappings;

internal static class BoardActionPositionMappings
{
    public static IReadOnlyList<BoardActionColumnPosition> ToColumnPositions(IEnumerable<Column> columns) =>
        columns
            .OrderBy(column => column.Position)
            .Select(column => new BoardActionColumnPosition(column.Id, column.Position))
            .ToList();

    public static IReadOnlyList<BoardActionTaskPosition> ToTaskPositions(IEnumerable<BoardTask> tasks) =>
        tasks
            .OrderBy(task => task.Position)
            .Select(task => new BoardActionTaskPosition(task.Id, task.ColumnId, task.Position))
            .ToList();
}
