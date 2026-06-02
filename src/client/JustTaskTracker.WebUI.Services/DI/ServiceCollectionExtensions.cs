using JustTaskTracker.WebUI.Services.Abstractions.Auth;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Services.Auth;
using JustTaskTracker.WebUI.Services.Auth.Stores;
using JustTaskTracker.WebUI.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace JustTaskTracker.WebUI.Services.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebUIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiClientOptions>(configuration.GetSection(ApiClientOptions.SectionName));
        var options = configuration.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()!;

        services.AddScoped<ApiAuthorizationMessageHandler>();

        services.AddRefitClient<IAuthApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(options.BaseUrl))
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

        services.AddScoped<IAuthApiService, AuthApiService>();
        services.AddScoped<IProfileStore, ProfileStore>();

        return services;
    }
}
