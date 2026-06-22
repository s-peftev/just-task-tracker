using JustTaskTracker.Application.Boards.Positioning;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI.Modules;

internal static class ServicesModule
{
    internal static IServiceCollection AddServicesModule(this IServiceCollection services)
    {
        services.AddScoped<IBoardPositioningService, BoardPositioningService>();

        return services;
    }
}
