using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Billing.Configurations;

public class StripeWebhookEventConfiguration : IEntityTypeConfiguration<StripeWebhookEvent>
{
    public void Configure(EntityTypeBuilder<StripeWebhookEvent> builder)
    {
        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventId).HasMaxLength(StripeWebhookEventFieldLengths.MaxEventIdLength);
        builder.Property(e => e.EventType).HasMaxLength(StripeWebhookEventFieldLengths.MaxEventTypeLength);
        builder.Property(e => e.LastError).HasMaxLength(StripeWebhookEventFieldLengths.MaxLastErrorLength);

        builder.HasIndex(e => e.ReceivedAtUtc)
            .HasFilter("[ProcessedAtUtc] IS NULL");
    }
}
