using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Calls;

internal sealed class CallsInteropService(IJSRuntime js) : ICallsInteropService, IAsyncDisposable
{
    private const string ModulePath = "./js/calls.js";

    private IJSObjectReference? _module;
    private IJSObjectReference? _callHandle;

    public async Task<CallEnvironmentCheckResult> CheckEnvironmentAsync()
    {
        var module = await EnsureModuleAsync();

        return await module.InvokeAsync<CallEnvironmentCheckResult>("checkEnvironment");
    }

    public async Task JoinRoomAsync(string token, string acsRoomId, ElementReference localVideoContainer, ElementReference remoteVideoContainer)
    {
        var module = await EnsureModuleAsync();

        _callHandle = await module.InvokeAsync<IJSObjectReference>(
            "join", token, acsRoomId, localVideoContainer, remoteVideoContainer);
    }

    public async Task HangUpAsync()
    {
        if (_callHandle is null)
            return;

        await _callHandle.InvokeVoidAsync("hangUp");
        await _callHandle.InvokeVoidAsync("dispose");
        await _callHandle.DisposeAsync();
        _callHandle = null;
    }

    private async Task<IJSObjectReference> EnsureModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);

    public async ValueTask DisposeAsync()
    {
        if (_callHandle is not null)
        {
            await _callHandle.InvokeVoidAsync("dispose");
            await _callHandle.DisposeAsync();
            _callHandle = null;
        }

        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }
    }
}
