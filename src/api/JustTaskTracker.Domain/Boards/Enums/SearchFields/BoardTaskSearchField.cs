using JustTaskTracker.Domain.Boards.Entities;
using System.Linq.Expressions;

namespace JustTaskTracker.Domain.Boards.Enums.SearchFields;

public enum BoardTaskSearchField : byte
{
    Title = 1,
    Description = 2
}

public static class BoardTaskSearchFields
{
    public static readonly IReadOnlyDictionary<BoardTaskSearchField, Expression<Func<BoardTask, string?>>> Map =
        new Dictionary<BoardTaskSearchField, Expression<Func<BoardTask, string?>>>
        {
            { BoardTaskSearchField.Title, task => task.Title },
            { BoardTaskSearchField.Description, task => task.Description }
        };
}
