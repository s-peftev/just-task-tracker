using Azure.Communication.Identity;
using Azure.Communication.Rooms;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Infrastructure.Calls.AzureCommunication;
using JustTaskTracker.Infrastructure.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class CallsModule
{
    internal static IServiceCollection AddCallsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.ACS)
            ?? throw new InvalidOperationException("Azure Communication Services connection string is not configured.");

        services.AddSingleton(new RoomsClient(connectionString));
        services.AddSingleton(new CommunicationIdentityClient(connectionString));

        services.AddScoped<IAcsCallProvisioningService, AcsCallProvisioningService>();

        return services;
    }
}
