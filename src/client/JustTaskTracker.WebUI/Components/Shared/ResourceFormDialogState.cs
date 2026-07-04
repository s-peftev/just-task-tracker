namespace JustTaskTracker.WebUI.Components.Shared;

public sealed class ResourceFormDialogState
{
    public bool DisableSubmit { get; private set; }

    public event Action? Changed;

    public void SetDisableSubmit(bool disableSubmit)
    {
        if (DisableSubmit == disableSubmit)
            return;

        DisableSubmit = disableSubmit;
        Changed?.Invoke();
    }
}
