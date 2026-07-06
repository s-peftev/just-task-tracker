using JustTaskTracker.Application.Boards.Notifiers;
using JustTaskTracker.Application.Common.Images;
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

        return services;
    }
}
