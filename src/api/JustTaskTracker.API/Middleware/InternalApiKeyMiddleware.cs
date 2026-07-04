using JustTaskTracker.Infrastructure.Archiving;

namespace JustTaskTracker.API.Middleware;

/// <summary>
/// Guards all /api/internal/** routes with a pre-shared API key.
/// Requests without a matching key are rejected with 401 before reaching MVC.
/// </summary>
public class InternalApiKeyMiddleware(RequestDelegate next, InternalApiOptions options)
{
    private static readonly PathString InternalPathPrefix = new("/api/internal");

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(InternalPathPrefix))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(options.ApiKeyHeaderName, out var providedKey)
            || !string.Equals(providedKey, options.ApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
