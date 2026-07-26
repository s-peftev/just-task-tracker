using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public record CallEnvironmentCheckResult(bool IsSupported, string? Reason);

public interface ICallsInteropService
{
    Task<CallEnvironmentCheckResult> CheckEnvironmentAsync();

    /// <summary>
    /// Joins the ACS Room. <paramref name="callbackRef"/>'s target must expose
    /// [JSInvokable] Task OnTileAdded(string tileId, bool isLocal) and
    /// [JSInvokable] Task OnTileRemoved(string tileId) -- called whenever a participant
    /// tile (the local user, tileId "local", or a remote participant) should appear/disappear.
    /// </summary>
    Task JoinRoomAsync<T>(string token, string acsRoomId, DotNetObjectReference<T> callbackRef) where T : class;

    /// <summary>
    /// Reports the rendered DOM element for a tile once Blazor has created it, so this service
    /// can render that tile's video stream into it (immediately, or later once the stream becomes available).
    /// </summary>
    Task RegisterTileElementAsync(string tileId, ElementReference element);

    Task UnregisterTileElementAsync(string tileId);

    Task<bool> ToggleMicAsync();

    Task<bool> ToggleCameraAsync();

    Task HangUpAsync();
}
