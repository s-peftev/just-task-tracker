using JustTaskTracker.WebUI.Services.Abstractions.Auth;
using JustTaskTracker.WebUI.Services.Abstractions.Boards;
using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Services.Auth;
using JustTaskTracker.WebUI.Services.Auth.Stores;
using JustTaskTracker.WebUI.Services.Configuration;
using JustTaskTracker.WebUI.Services.Boards;
using JustTaskTracker.WebUI.Services.Boards.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Text.Json;

namespace JustTaskTracker.WebUI.Services.DI;

public static class ServiceCollectionExtensions
{
    private static readonly RefitSettings RefitSettings = new()
    {
        ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
    };

    public static IServiceCollection AddWebUIServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiClientOptions>(configuration.GetSection(ApiClientOptions.SectionName));
        var options = configuration.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()!;

        services.AddScoped<ApiAuthorizationMessageHandler>();

        services.AddRefitClient<IAuthApi>(RefitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(options.BaseUrl))
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

        services.AddRefitClient<IBoardApi>(RefitSettings)
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(options.BaseUrl))
            .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

        services.AddScoped<IAuthApiService, AuthApiService>();
        services.AddScoped<IProfileStore, ProfileStore>();

        services.AddScoped<IBoardApiService, BoardApiService>();
        services.AddScoped<IBoardStore, BoardStore>();
        services.AddScoped<IBoardDetailsStore, BoardDetailsStore>();

        return services;
    }
}
