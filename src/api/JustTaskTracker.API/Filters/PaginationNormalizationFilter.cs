using JustTaskTracker.Domain.Common;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JustTaskTracker.API.Filters;

/// <summary>
/// Normalizes <see cref="PaginatedRequest"/> instances in-place before the action runs: fills defaults for invalid page values
/// and clamps <see cref="PaginatedRequest.PageSize"/> to <see cref="PaginationOptions.MaxPageSize"/> to bound query cost.
/// </summary>
public class PaginationNormalizationFilter(PaginationDefaultsOptions opts) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext ctx)
    {
        foreach (var arg in ctx.ActionArguments.Values)
        {
            if (arg is PaginatedRequest p)
            {
                p.PageNumber = p.PageNumber <= 0
                    ? opts.DefaultPageNumber
                    : p.PageNumber;

                p.PageSize = p.PageSize <= 0
                    ? opts.DefaultPageSize
                    : Math.Min(p.PageSize, opts.MaxPageSize);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext ctx) { }
}
