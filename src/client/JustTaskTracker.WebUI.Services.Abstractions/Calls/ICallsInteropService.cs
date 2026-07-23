using Microsoft.AspNetCore.Components;

namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public record CallEnvironmentCheckResult(bool IsSupported, string? Reason);

public interface ICallsInteropService
{
    Task<CallEnvironmentCheckResult> CheckEnvironmentAsync();

    Task JoinRoomAsync(string token, string acsRoomId, ElementReference localVideoContainer, ElementReference remoteVideoContainer);

    Task HangUpAsync();
}
