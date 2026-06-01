using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Infrastructure.Auth;
using JustTaskTracker.Infrastructure.Auth.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options => 
        {
            var clientId = configuration["AzureAd:ClientId"];

            options.TokenValidationParameters.ValidAudiences =
            [
                clientId,
                $"api://{clientId}" 
            ];

            options.TokenValidationParameters.RoleClaimType = EntraClaimTypes.Roles;
            options.TokenValidationParameters.NameClaimType = EntraClaimTypes.ObjectId;
        });

        return services;
    }
}
