using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Common.Images;
using JustTaskTracker.Infrastructure.Billing;
using JustTaskTracker.Infrastructure.Boards.Notifiers;
using JustTaskTracker.Infrastructure.Common.Images;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class ServicesModule
{
    internal static IServiceCollection AddServicesModule(this IServiceCollection services)
    {
        services.AddSingleton<IImageProcessor, ImageProcessor>();
        services.AddScoped<IBoardExportStatusNotifier, BoardExportStatusNotifier>();
        services.AddScoped<IBoardActionNotifier, BoardActionNotifier>();
        services.AddSingleton<IPlanCatalog, PlanCatalog>();
        services.AddScoped<IEntitlementService, EntitlementService>();

        return services;
    }
}
