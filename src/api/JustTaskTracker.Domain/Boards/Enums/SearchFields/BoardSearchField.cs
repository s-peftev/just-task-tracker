using JustTaskTracker.Domain.Boards.Entities;
using System.Linq.Expressions;

namespace JustTaskTracker.Domain.Boards.Enums.SearchFields;

public enum BoardSearchField : byte
{
    Name = 1
}

public static class BoardSearchFields
{
    public static readonly IReadOnlyDictionary<BoardSearchField, Expression<Func<Board, string?>>> Map =
        new Dictionary<BoardSearchField, Expression<Func<Board, string?>>>
        {
            { BoardSearchField.Name, b => b.Name }
        };
}
