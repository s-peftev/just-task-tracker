namespace JustTaskTracker.Domain.Billing.Constants;

public static class SubscriptionFieldLengths
{
    public const int MaxPlanIdLength = 64;
    public const int MaxStripeCustomerIdLength = 255;
    public const int MaxStripeSubscriptionIdLength = 255;
    public const int MaxStatusLength = 32;
}
