namespace JustTaskTracker.Domain.Billing.Constants;

public static class StripeWebhookEventFieldLengths
{
    public const int MaxEventIdLength = 255;
    public const int MaxEventTypeLength = 128;
    public const int MaxLastErrorLength = 2000;
}
