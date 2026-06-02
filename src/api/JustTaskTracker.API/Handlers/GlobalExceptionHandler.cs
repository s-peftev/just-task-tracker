using JustTaskTracker.API.Models;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace JustTaskTracker.API.Handlers;

// <summary>
/// Maps unhandled exceptions to HTTP status and a failure <see cref="ApiResponse{T}"/> body written as JSON.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, error) = GetExceptionDetails(exception);

        httpContext.Response.StatusCode = (int)statusCode;
        var response = ApiResponse<object>.Fail(error);

        await httpContext.Response.WriteAsJsonAsync(response, ct);

        return true;
    }

    /// <summary>
    /// <see cref="OperationCanceledException"/> is mapped to 400 (client cancellation / aborted request) rather than 499 or 503,
    /// which keeps the contract simple for API clients that cancel in-flight calls.
    /// </summary>
    private static (HttpStatusCode statusCode, Error error) GetExceptionDetails(Exception exception) =>
        exception switch
        {
            OperationCanceledException _ => (HttpStatusCode.BadRequest, ExceptionErrors.RequestCancelled),
            TimeoutException _ => (HttpStatusCode.RequestTimeout, ExceptionErrors.Timeout),
            _ => (HttpStatusCode.InternalServerError, GeneralErrors.InternalServerError)
        };
}
