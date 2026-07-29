using JustTaskTracker.WebUI.Services.Abstractions.Calls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Calls;

internal sealed class CallsInteropService(IJSRuntime js) : ICallsInteropService, IAsyncDisposable
{
    private const string ModulePath = "./js/calls.js";

    private IJSObjectReference? _module;

    public CallPreJoinMediaPreferences PreJoin { get; } = new();

    public async Task<CallEnvironmentCheckResult> CheckEnvironmentAsync()
    {
        var module = await EnsureModuleAsync();

        return await module.InvokeAsync<CallEnvironmentCheckResult>("checkEnvironment");
    }

    public async Task<CallPreJoinDevicesResult> GetPreJoinDevicesAsync()
    {
        var module = await EnsureModuleAsync();
        var result = await module.InvokeAsync<CallPreJoinDevicesResult>("getPreJoinDevices");

        if (string.IsNullOrWhiteSpace(PreJoin.CameraDeviceId)
            && !string.IsNullOrWhiteSpace(result.SelectedCameraId))
            PreJoin.CameraDeviceId = result.SelectedCameraId;

        if (string.IsNullOrWhiteSpace(PreJoin.MicrophoneDeviceId)
            && !string.IsNullOrWhiteSpace(result.SelectedMicrophoneId))
            PreJoin.MicrophoneDeviceId = result.SelectedMicrophoneId;

        return result with
        {
            SelectedCameraId = PreJoin.CameraDeviceId ?? result.SelectedCameraId,
            SelectedMicrophoneId = PreJoin.MicrophoneDeviceId ?? result.SelectedMicrophoneId,
        };
    }

    public async Task JoinRoomAsync<T>(string token, string acsRoomId, DotNetObjectReference<T> callbackRef) where T : class
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync(
            "join",
            token,
            acsRoomId,
            callbackRef,
            new
            {
                micEnabled = PreJoin.MicEnabled,
                cameraEnabled = PreJoin.CameraEnabled,
                microphoneDeviceId = PreJoin.MicrophoneDeviceId,
                cameraDeviceId = PreJoin.CameraDeviceId,
            });
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

    public async Task RegisterStageElementAsync(ElementReference element)
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("registerStageElement", element);
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

    public async Task StartScreenShareAsync()
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("startScreenSharing");
    }

    public async Task StopScreenShareAsync()
    {
        var module = await EnsureModuleAsync();

        await module.InvokeVoidAsync("stopScreenSharing");
    }

    public async Task HangUpAsync()
    {
        if (_module is null)
            return;

        await _module.InvokeVoidAsync("hangUp");
        await _module.InvokeVoidAsync("disposeCall");
    }

    private async Task<IJSObjectReference> EnsureModuleAsync()
    {
        if (_module is not null)
            return _module;

        try
        {
            _module = await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
            return _module;
        }
        catch
        {
            // Drop any partial reference so the next attempt can re-import after a page refresh
            // replaces a stale fingerprinted asset URL.
            _module = null;
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
            _module = null;
        }
    }
}
