using JustTaskTracker.API.Models;
using JustTaskTracker.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JustTaskTracker.API.Filters;

/// <summary>
/// Wraps successful MVC results in <see cref="ApiResponse{T}"/> so clients always receive a consistent envelope.
/// Skips wrapping when the payload is already an <see cref="ApiResponse{T}"/> or when the value is an <see cref="Error"/> (converted to a failure envelope).
/// </summary>
public sealed class ApiResponseEnvelopeFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        context.Result = ApplyEnvelope(context.Result);

        return next();
    }

    private static IActionResult ApplyEnvelope(IActionResult result)
    {
        return result switch
        {
            OkResult => new OkObjectResult(ApiResponse<object>.Ok()),
            ObjectResult objectResult => ApplyToObjectResult(objectResult),
            _ => result,
        };
    }

    private static ObjectResult ApplyToObjectResult(ObjectResult objectResult)
    {
        if (objectResult.Value is Error error)
        {
            objectResult.Value = ApiResponse<object>.Fail(error);

            return objectResult;
        }

        if (ShouldSkipWrapping(objectResult.Value))
            return objectResult;

        objectResult.Value = objectResult.Value is null
            ? ApiResponse<object>.Ok()
            : ApiResponse<object>.Ok(objectResult.Value);

        return objectResult;
    }

    private static bool ShouldSkipWrapping(object? value)
    {
        if (value is null)
            return false;

        return IsApiResponseEnvelope(value);
    }

    private static bool IsApiResponseEnvelope(object value)
    {
        var type = value.GetType();

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
    }
}
