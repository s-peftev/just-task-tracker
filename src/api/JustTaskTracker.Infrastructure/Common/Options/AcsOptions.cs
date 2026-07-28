using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class AcsOptions
{
    // AD-11: shared secret appended as a query-string parameter (?validationKey=...) on the Event
    // Grid subscription's webhook endpoint URL -- works unchanged whether that URL currently points
    // at a local tunnel or, after redeploying the subscription, the cloud endpoint.
    public string WebhookValidationKey { get; set; } = string.Empty;

    public void Validate()
    {
        var section = ConfigSections.Acs;

        if (string.IsNullOrWhiteSpace(WebhookValidationKey))
            throw new InvalidOperationException($"{section}:WebhookValidationKey is not configured.");
    }
}
