using JustTaskTracker.API.Extensions;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Results.Errors;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JustTaskTracker.API.Filters;

/// <summary>
/// Validates <see cref="PaginatedRequest"/> query parameters before the action runs.
/// Omitted values receive configured defaults; explicitly invalid values short-circuit with 400.
/// </summary>
public sealed class PaginationValidationFilter(PaginationDefaultsOptions opts) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is not PaginatedRequest request)
                continue;

            var errors = Validate(request);
            if (errors.Count > 0)
            {
                context.Result = (GeneralErrors.InvalidRequest with { Details = errors }).CreateErrorResponse();
                return;
            }

            request.PageNumber = request.PageNumber ?? opts.DefaultPageNumber;
            request.PageSize = request.PageSize ?? opts.DefaultPageSize;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    private List<string> Validate(PaginatedRequest request)
    {
        var errors = new List<string>();

        if (request.PageNumber is < 1)
            errors.Add("PageNumber must be >= 1.");

        if (request.PageSize is < 1)
            errors.Add("PageSize must be >= 1.");

        if (request.PageSize is { } pageSize && pageSize > opts.MaxPageSize)
            errors.Add($"PageSize cannot exceed {opts.MaxPageSize}. Requested: {pageSize}.");

        return errors;
    }
}
