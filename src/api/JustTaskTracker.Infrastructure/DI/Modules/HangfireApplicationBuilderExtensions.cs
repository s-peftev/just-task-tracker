using Hangfire;
using JustTaskTracker.Application.Boards.Jobs;
using JustTaskTracker.Application.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

public static class HangfireApplicationBuilderExtensions
{
    public static WebApplication UseHangfireDashboardModule(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire");

        RegisterRecurringJobs(app);

        return app;
    }

    private static void RegisterRecurringJobs(WebApplication app)
    {
        var schedulerOptions = app.Services.GetRequiredService<BoardExportSchedulerOptions>();

        RecurringJob.AddOrUpdate<IBoardExportSchedulerJob>(
            recurringJobId: "board-export-scheduler",
            methodCall: job => job.RunAsync(CancellationToken.None),
            cronExpression: schedulerOptions.CronExpression);
    }
}
