using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class CorsModule
{
    internal static IServiceCollection AddCorsModule(this IServiceCollection services)
    {
        services.AddCors();

        services.AddOptions<CorsOptions>().Configure<FrontendOptions>((corsOptions, frontendOptions) =>
        {
            var origins = frontendOptions?.AllowedOrigins;

            var policyBuilder = new CorsPolicyBuilder()
                .AllowAnyHeader()
                .AllowAnyMethod();

            if (origins is not null && origins.Length > 0)
            {
                policyBuilder.WithOrigins(origins);
            }
            else
            {
                policyBuilder.AllowAnyOrigin();
            }

            corsOptions.AddPolicy(CorsPolicies.DefaultCorsPolicy, policyBuilder.Build());
        });

        return services;
    }
}
