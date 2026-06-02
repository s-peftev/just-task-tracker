using JustTaskTracker.Infrastructure.Common.Constants;
using Serilog.Context;

namespace JustTaskTracker.API.Middleware;

/// <summary>
/// Ensures each request has a correlation id: reuses the incoming header or generates one, echoes it on the response,
/// stores it on <see cref="HttpContext.Items"/> for app code, and pushes it into Serilog <c>LogContext</c> for the rest of the pipeline.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string headerName = HttpHeaders.XCorrelationId;

        var correlationId = context.Request.Headers[headerName].FirstOrDefault() ?? Guid.NewGuid().ToString();

        context.Response.Headers[headerName] = correlationId;
        context.Items[headerName] = correlationId;

        using (LogContext.PushProperty(LogProperties.CorrelationId, correlationId))
        {
            await next(context);
        }
    }
}

