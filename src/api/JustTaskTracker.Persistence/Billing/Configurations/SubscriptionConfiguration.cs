using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Domain.Billing.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Billing.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(s => s.PlanId).HasMaxLength(SubscriptionFieldLengths.MaxPlanIdLength);
        builder.Property(s => s.StripeCustomerId).HasMaxLength(SubscriptionFieldLengths.MaxStripeCustomerIdLength);
        builder.Property(s => s.StripeSubscriptionId).HasMaxLength(SubscriptionFieldLengths.MaxStripeSubscriptionIdLength);
        builder.Property(s => s.Status).HasMaxLength(SubscriptionFieldLengths.MaxStatusLength);
        builder.Property(s => s.CancelAtPeriodEnd).HasDefaultValue(false);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => s.StripeSubscriptionId)
            .IsUnique();

        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasFilter(BuildBillableStatusFilter());

        builder.HasIndex(s => s.UserId);

        builder.HasIndex(s => s.StripeCustomerId);
    }

    private static string BuildBillableStatusFilter()
    {
        var statuses = string.Join(", ", SubscriptionStatus.AllBillable.Select(s => $"N'{s}'"));
        return $"[Status] IN ({statuses})";
    }
}
