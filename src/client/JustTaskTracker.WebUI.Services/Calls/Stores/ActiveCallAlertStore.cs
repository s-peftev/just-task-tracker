using JustTaskTracker.WebUI.Domain.Calls.Notifications;
using JustTaskTracker.WebUI.Services.Abstractions.Calls;

namespace JustTaskTracker.WebUI.Services.Calls.Stores;

internal sealed class ActiveCallAlertStore : IActiveCallAlertStore
{
    public event Action<CallStartedAlert>? CallStarted;

    public void ApplyCallStartedAlert(CallStartedAlert alert) => CallStarted?.Invoke(alert);
}
