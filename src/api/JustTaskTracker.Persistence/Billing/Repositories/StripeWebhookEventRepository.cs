using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace JustTaskTracker.Persistence.Billing.Repositories;

public class StripeWebhookEventRepository(JustTaskTrackerDbContext context) : IStripeWebhookEventRepository
{
    public Task<StripeWebhookEvent?> GetByEventIdAsync(string eventId, CancellationToken ct = default) =>
        context.StripeWebhookEvents.FirstOrDefaultAsync(e => e.EventId == eventId, ct);

    public void Add(StripeWebhookEvent webhookEvent) =>
        context.StripeWebhookEvents.Add(webhookEvent);
}
