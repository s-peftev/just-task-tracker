using System.Linq.Expressions;

namespace JustTaskTracker.Application.Common.Helpers;

public static class SearchFieldsResolver
{
    public static IReadOnlyCollection<Expression<Func<TEntity, string?>>> Resolve<TEntity, TField>(
        IReadOnlyCollection<TField>? requested,
        IReadOnlyDictionary<TField, Expression<Func<TEntity, string?>>> map)
        where TField : struct, Enum
    {
        if (requested is { Count: > 0 })
        {
            return requested
                .Distinct()
                .Select(f => map.TryGetValue(f, out var expr)
                    ? expr
                    : throw new ArgumentOutOfRangeException(nameof(requested), f, $"Field {f} is not mapped for {typeof(TEntity).Name}."))
                .ToArray();
        }

        return map.Values.ToArray();
    }
}
