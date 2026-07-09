namespace JustTaskTracker.Domain.Billing.Entities;

public class StripeWebhookEvent
{
    public required string EventId { get; init; }
    public required string EventType { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public string? LastError { get; set; }
}
