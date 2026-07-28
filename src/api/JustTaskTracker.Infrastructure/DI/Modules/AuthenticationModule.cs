using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Domain.Auth.Constants;
using JustTaskTracker.Infrastructure.Auth;
using JustTaskTracker.Infrastructure.Auth.Constants;
using JustTaskTracker.Infrastructure.Calls.Auth;
using JustTaskTracker.Infrastructure.Common.Constants.Hubs;
using JustTaskTracker.Infrastructure.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;

namespace JustTaskTracker.Infrastructure.DI.Modules;

internal static class AuthenticationModule
{
    internal static IServiceCollection AddAuthenticationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        // AD-10: SignalR's own user-targeting (Clients.User(...)) needs to know how to read a
        // connection's identity -- same "oid" claim as ICurrentUserAccessor below, just SignalR's
        // side of it. Registered here (not in AzureModule) because this is an identity-claims
        // concern, not an Azure-resource-wiring one; still runs before AddSignalR() either way,
        // since AddAuthenticationModule is called before AddAzureModule in AddInfrastructure.
        services.AddSingleton<IUserIdProvider, AzureAdObjectIdUserIdProvider>();

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection(Common.Constants.ConfigSections.AzureAd));

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<AzureAdOptions>((options, azureAdOptions) =>
            {
                var clientId = azureAdOptions.ClientId;

                options.TokenValidationParameters.ValidAudiences =
                [
                    clientId,
                    $"api://{clientId}"
                ];

                options.TokenValidationParameters.RoleClaimType = EntraClaimTypes.Roles;
                options.TokenValidationParameters.NameClaimType = EntraClaimTypes.ObjectId;

                ConfigureSignalRJwtBearerEvents(options);
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.IsAppAdmin, policy => policy.RequireRole(Roles.Admin.ToString()))
            .AddPolicy(AuthorizationPolicies.IsAppContributor, policy => policy.RequireRole(Roles.Admin.ToString(), Roles.User.ToString()))
            .AddPolicy(AuthorizationPolicies.IsAppUser, policy => policy.RequireRole(Roles.User.ToString()))
            .AddPolicy(AuthorizationPolicies.IsAppMember, policy => policy.RequireRole(Roles.Admin.ToString(), Roles.User.ToString(), Roles.Guest.ToString()));

        return services;
    }

    private static void ConfigureSignalRJwtBearerEvents(JwtBearerOptions options)
    {
        var previousOnMessageReceived = options.Events.OnMessageReceived;

        options.Events.OnMessageReceived = async context =>
        {
            if (string.IsNullOrEmpty(context.Token)
                && context.HttpContext.Request.Path.StartsWithSegments(HubPaths.Root))
            {
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken))
                    context.Token = accessToken;
            }

            if (previousOnMessageReceived is not null)
                await previousOnMessageReceived(context);
        };
    }
}
