using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Boards.Entities;
using System.Linq.Expressions;

namespace JustTaskTracker.Domain.Boards.Enums.SearchFields;

public enum BoardMemberSearchField : byte
{
    DisplayName = 1,
    Email = 2
}

public static class BoardMemberSearchFields
{
    public static readonly IReadOnlyDictionary<BoardMemberSearchField, Expression<Func<BoardMember, string?>>> Map =
        new Dictionary<BoardMemberSearchField, Expression<Func<BoardMember, string?>>>
        {
            { BoardMemberSearchField.DisplayName, member => member.User!.DisplayName },
            { BoardMemberSearchField.Email, member => member.User!.Email }
        };
}
