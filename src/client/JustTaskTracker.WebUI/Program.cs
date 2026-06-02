using JustTaskTracker.WebUI;
using JustTaskTracker.WebUI.Services.Configuration;
using JustTaskTracker.WebUI.Services.DI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddWebUIServices(builder.Configuration);

var apiOptions = builder.Configuration.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()!;

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    foreach (var scope in apiOptions.Scopes)
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
    options.ProviderOptions.LoginMode = "redirect";
});

await builder.Build().RunAsync();