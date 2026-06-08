using JustTaskTracker.Application.Common.Logging;
using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request name and parameters on entry,
/// logs success summaries or failures on exit, and logs unhandled exceptions before rethrowing.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<MediatrPipelineLogging> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        LogRequestStarted(requestName, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(ct);
            stopwatch.Stop();

            if (!response.IsSuccess)
            {
                LogFailure(requestName, response.Error, stopwatch.ElapsedMilliseconds);
                return response;
            }

            LogSuccess(requestName, response, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogException(ex, requestName, request, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void LogRequestStarted(string requestName, TRequest request) =>
        logger.LogInformation(
            "Handling {RequestName} {@Request}",
            requestName,
            request);

    private void LogSuccess(string requestName, TResponse response, long durationMs) =>
        logger.LogInformation(
            "Handled {RequestName} in {DurationMs}ms. Response: {ResponseSummary}",
            requestName,
            durationMs,
            ResultLogSummaryFormatter.FormatSuccessResponse(response));

    private void LogFailure(string requestName, Error error, long durationMs) =>
        logger.Log(
            GetFailureLogLevel(error.Type),
            "Request {RequestName} failed after {DurationMs}ms with {ErrorCode} ({ErrorType}). {ErrorDetails}",
            requestName,
            durationMs,
            error.Code,
            error.Type,
            error.Details);

    private void LogException(Exception exception, string requestName, TRequest request, long durationMs) =>
        logger.LogError(
            exception,
            "Unhandled exception in {RequestName} after {DurationMs}ms {@Request}",
            requestName,
            durationMs,
            request);

    private static LogLevel GetFailureLogLevel(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.InternalServerError or ErrorType.ServiceUnavailable => LogLevel.Error,
            _ => LogLevel.Warning,
        };
}
