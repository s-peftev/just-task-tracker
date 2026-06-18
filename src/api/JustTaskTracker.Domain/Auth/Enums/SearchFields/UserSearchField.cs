using JustTaskTracker.Domain.Auth.Entities;
using System.Linq.Expressions;

namespace JustTaskTracker.Domain.Auth.Enums.SearchFields;

public enum UserSearchField : byte
{
    Email = 1,
    DisplayName = 2
}

public static class UserSearchFields
{
    public static readonly IReadOnlyDictionary<UserSearchField, Expression<Func<User, string?>>> Map =
        new Dictionary<UserSearchField, Expression<Func<User, string?>>>
        {
            { UserSearchField.DisplayName, user => user.DisplayName },
            { UserSearchField.Email, user => user.Email }
        };
}