using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace JustTaskTracker.Infrastructure.DI.Modules;

public static class HangfireApplicationBuilderExtensions
{
    public static WebApplication UseHangfireDashboardModule(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire");

        return app;
    }
}
