using BlazorSortable;
using Cropper.Blazor.Extensions;
using JustTaskTracker.WebUI;
using JustTaskTracker.WebUI.Services.Configuration;
using JustTaskTracker.WebUI.Services.DI;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.PreventDuplicates = false;
});
builder.Services.AddWebUIServices(builder.Configuration);
builder.Services.AddCropper();
builder.Services.AddSortable(options =>
{
    options.Defaults.Delay = 0;
    options.Defaults.DelayOnTouchOnly = true;
    options.Defaults.FallbackTolerance = 5;
});

var apiOptions = builder.Configuration.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()!;

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    foreach (var scope in apiOptions.Scopes)
        options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
    options.ProviderOptions.LoginMode = "redirect";
});

await builder.Build().RunAsync();