namespace JustTaskTracker.WebUI.Services.Abstractions.Calls;

public record CallMediaDeviceDto(string Id, string Name);

public record CallPreJoinDevicesResult(
    IReadOnlyList<CallMediaDeviceDto> Cameras,
    IReadOnlyList<CallMediaDeviceDto> Microphones,
    string? SelectedCameraId,
    string? SelectedMicrophoneId);

public sealed class CallPreJoinMediaPreferences
{
    public bool MicEnabled { get; set; } = true;
    public bool CameraEnabled { get; set; } = true;
    public string? MicrophoneDeviceId { get; set; }
    public string? CameraDeviceId { get; set; }
}
