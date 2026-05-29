using JustTaskTracker.Infrastructure.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class CorsModule
{
    public static IServiceCollection AddCorsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Frontend:Url").Value
            ?? throw new InvalidOperationException("FrontEnd Url is not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicies.DefaultCorsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins!)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
