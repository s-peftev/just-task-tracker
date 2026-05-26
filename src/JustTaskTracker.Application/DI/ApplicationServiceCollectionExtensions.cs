using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly));

        return services;
    }
}
