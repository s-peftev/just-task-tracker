using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public record CallEnvironmentCheckResult(bool IsSupported, string? Reason);

public interface ICallsInteropService
{
    CallPreJoinMediaPreferences PreJoin { get; }

    Task<CallEnvironmentCheckResult> CheckEnvironmentAsync();

    Task<CallPreJoinDevicesResult> GetPreJoinDevicesAsync();

    /// <summary>
    /// Joins the ACS Room. <paramref name="callbackRef"/>'s target must expose
    /// [JSInvokable] Task OnTileAdded(string tileId, bool isLocal, bool hasVideo),
    /// [JSInvokable] Task OnTileRemoved(string tileId),
    /// [JSInvokable] Task OnTileVideoStateChanged(string tileId, bool hasVideo) -- called whenever a
    /// participant tile (the local user, tileId "local", or a remote participant) should appear/disappear,
    /// or a remote participant's camera/video stream availability changes after the tile was added,
    /// [JSInvokable] Task OnCallDisconnected() -- called if the local ACS call state goes Disconnected
    /// for any reason other than this client's own hangUp()/dispose (e.g. a force-end, AD-15), and
    /// [JSInvokable] Task OnLocalScreenShareStopped() -- called if the local user's screen share stops
    /// through a means other than this service's own StopScreenShareAsync (e.g. the browser's native
    /// "Stop sharing" control), so the presenter lock (AD-9) doesn't get left stuck server-side.
    /// </summary>
    Task JoinRoomAsync<T>(string token, string acsRoomId, DotNetObjectReference<T> callbackRef) where T : class;

    /// <summary>
    /// Reports the rendered DOM element for a tile once Blazor has created it, so this service
    /// can render that tile's video stream into it (immediately, or later once the stream becomes available).
    /// </summary>
    Task RegisterTileElementAsync(string tileId, ElementReference element);

    Task UnregisterTileElementAsync(string tileId);

    /// <summary>
    /// Reports the rendered DOM element for the "stage" (the current presenter's screen-share view),
    /// so this service can render whichever remote participant's screen-sharing stream shows up into it.
    /// </summary>
    Task RegisterStageElementAsync(ElementReference element);

    Task<bool> ToggleMicAsync();

    Task<bool> ToggleCameraAsync();

    /// <summary>
    /// Starts local screen sharing through the ACS SDK. Call only after the server has already
    /// granted the presenter lock (AD-9) -- never the other way around.
    /// </summary>
    Task StartScreenShareAsync();

    Task StopScreenShareAsync();

    Task HangUpAsync();
}
