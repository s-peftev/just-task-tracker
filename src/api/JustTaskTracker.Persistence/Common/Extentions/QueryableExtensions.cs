using JustTaskTracker.Domain.Common.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JustTaskTracker.Persistence.Common.Extentions;

public static class QueryableExtensions
{
    /// <summary>
    /// Paginates the given <see cref="IQueryable{TItem}"/> by applying skip and take based on the specified page number and page size.
    /// </summary>
    /// <typeparam name="TItem">The element type of the query.</typeparam>
    /// <param name="query">The source query to paginate.</param>
    /// <param name="pageNumber">The 1-based page number. Values less than 1 will skip 0 items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// A <see cref="PagedList{TItem}"/> containing the current page, page size, total count of items, and the items for the current page.
    /// </returns>
    public static async Task<PagedList<TItem>> ToPagedAsync<TItem>(
        this IQueryable<TItem> query,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<TItem>(pageNumber, pageSize, totalCount, items);
    }

    /// <summary>
    /// Paginates the given <see cref="IQueryable{TSource}"/> and projects the resulting page into <typeparamref name="TResult"/>.
    /// The total count is evaluated against the source query before projection, and the projection is applied only
    /// to the items of the current page, so expensive selectors run for the page rows instead of the whole result set.
    /// </summary>
    /// <typeparam name="TSource">The element type of the source query.</typeparam>
    /// <typeparam name="TResult">The element type produced by the projection.</typeparam>
    /// <param name="source">The source query to paginate.</param>
    /// <param name="selector">The projection applied to the items of the current page.</param>
    /// <param name="pageNumber">The 1-based page number. Values less than 1 will skip 0 items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// A <see cref="PagedList{TResult}"/> containing the current page, page size, total count of items, and the projected items for the current page.
    /// </returns>
    public static async Task<PagedList<TResult>> ToPagedAsync<TSource, TResult>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TResult>> selector,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await source.CountAsync(ct);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(ct);

        return new PagedList<TResult>(pageNumber, pageSize, totalCount, items);
    }

    /// <summary>
    /// Applies a text search filter to the given <see cref="IQueryable{TItem}"/> based on the specified fields.
    /// </summary>
    /// <typeparam name="TItem">The entity type of the query.</typeparam>
    /// <param name="query">The source query to filter.</param>
    /// <param name="search">The search string to match. If null or whitespace, the query is returned unmodified.</param>
    /// <param name="fields">
    /// The collection of string fields to search in. Each field is an expression selecting a string property from <typeparamref name="TItem"/>.
    /// If empty, the query is returned unmodified.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IQueryable{TItem}"/> where any of the specified fields contain the search string, ignoring null values.
    /// Multiple fields are combined with logical OR.
    /// </returns>
    public static IQueryable<TItem> ApplyTextSearch<TItem>(this IQueryable<TItem> query, string? search, IReadOnlyCollection<Expression<Func<TItem, string?>>> fields)
    {
        if (string.IsNullOrWhiteSpace(search) || fields.Count == 0)
            return query;

        string parameterName = fields.First().Parameters.FirstOrDefault()?.Name ?? "x";

        var parameter = Expression.Parameter(typeof(TItem), parameterName);
        Expression? body = null;

        foreach (var field in fields)
        {
            var replaced = ReplaceParameter(field, parameter);

            var notNull = Expression.NotEqual(
                replaced,
                Expression.Constant(null, typeof(string))
            );

            var contains = Expression.Call(
                replaced,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(search)
            );

            var condition = Expression.AndAlso(notNull, contains);

            body = body is null
                ? condition
                : Expression.OrElse(body, condition);
        }

        var lambda = Expression.Lambda<Func<TItem, bool>>(body!, parameter);

        return query.Where(lambda);
    }

    private static Expression ReplaceParameter<T>(Expression<Func<T, string?>> expr, ParameterExpression parameter)
    {
        return new ParameterReplaceVisitor(expr.Parameters[0], parameter)
            .Visit(expr.Body)!;
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        private readonly ParameterExpression _from = from;
        private readonly ParameterExpression _to = to;

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }
}
