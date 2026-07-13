using JustTaskTracker.Domain.Billing.Entities;

namespace JustTaskTracker.Application.Billing.Repositories;

public interface IStripeWebhookEventRepository
{
    Task<StripeWebhookEvent?> GetByEventIdAsync(string eventId, CancellationToken ct = default);

    void Add(StripeWebhookEvent webhookEvent);
}
