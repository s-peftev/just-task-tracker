namespace JustTaskTracker.WebUI.Domain.Billing;

public enum PaymentConfirmationStatus
{
    Idle = 0,
    Confirming = 1,
    Confirmed = 2,
    AwaitingActivation = 3,
}
