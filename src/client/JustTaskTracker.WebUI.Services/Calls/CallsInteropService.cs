using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Calls;

internal sealed class CallsInteropService(IJSRuntime js) : ICallsInteropService, IAsyncDisposable
{
    private const string ModulePath = "./js/calls.js";

    private IJSObjectReference? _module;

    public async Task<CallEnvironmentCheckResult> CheckEnvironmentAsync()
    {
        var module = await EnsureModuleAsync();

        return await module.InvokeAsync<CallEnvironmentCheckResult>("checkEnvironment");
    }

    public async Task JoinRoomAsync<T>(string token, string acsRoomId, DotNetObjectReference<T> callbackRef) where T : class
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("join", token, acsRoomId, callbackRef);
    }

    public async Task RegisterTileElementAsync(string tileId, ElementReference element)
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("registerTileElement", tileId, element);
    }

    public async Task UnregisterTileElementAsync(string tileId)
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("unregisterTileElement", tileId);
    }

    public async Task<bool> ToggleMicAsync()
    {
        var module = await EnsureModuleAsync();

        return await module.InvokeAsync<bool>("toggleMic");
    }

    public async Task<bool> ToggleCameraAsync()
    {
        var module = await EnsureModuleAsync();

        return await module.InvokeAsync<bool>("toggleCamera");
    }

    public async Task HangUpAsync()
    {
        if (_module is null)
            return;

        await _module.InvokeVoidAsync("hangUp");
        await _module.InvokeVoidAsync("disposeCall");
    }

    private async Task<IJSObjectReference> EnsureModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }
    }
}
