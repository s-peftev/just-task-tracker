using JustTaskTracker.WebUI.Services.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace JustTaskTracker.WebUI.Services.Auth;

internal sealed class ApiAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public ApiAuthorizationMessageHandler(
        IAccessTokenProvider provider,
        NavigationManager navigation,
        IOptions<ApiClientOptions> options)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: [options.Value.BaseUrl],
            scopes: options.Value.Scopes);
    }
}
