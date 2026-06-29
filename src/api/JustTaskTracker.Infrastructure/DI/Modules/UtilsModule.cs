using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Infrastructure.Common.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class UtilsModule
{
    internal static IServiceCollection AddUtilsModule(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
