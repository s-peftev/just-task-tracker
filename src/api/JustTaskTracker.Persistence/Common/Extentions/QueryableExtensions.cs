using JustTaskTracker.Domain.Common;
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
}
