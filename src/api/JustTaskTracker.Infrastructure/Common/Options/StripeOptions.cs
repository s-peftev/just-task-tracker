using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;

    public void Validate()
    {
        var section = ConfigSections.Stripe;

        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException($"{section}:SecretKey is not configured.");

        if (string.IsNullOrWhiteSpace(WebhookSecret))
            throw new InvalidOperationException($"{section}:WebhookSecret is not configured.");

        if (string.IsNullOrWhiteSpace(SuccessUrl))
            throw new InvalidOperationException($"{section}:SuccessUrl is not configured.");

        if (string.IsNullOrWhiteSpace(CancelUrl))
            throw new InvalidOperationException($"{section}:CancelUrl is not configured.");
    }
}
