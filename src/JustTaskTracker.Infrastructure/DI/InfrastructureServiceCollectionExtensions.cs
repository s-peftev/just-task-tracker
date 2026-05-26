using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        return services;
    }
}
