using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Domain.Billing.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantRequesterAccountReadModelConfiguration
    : IEntityTypeConfiguration<AssistantRequesterAccountReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantRequesterAccountReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_RequesterAccount");

        builder.Property(x => x.GlobalRoles).HasMaxLength(64);
        builder.Property(x => x.PlanId).HasMaxLength(SubscriptionFieldLengths.MaxPlanIdLength);
        builder.Property(x => x.SubscriptionStatus).HasMaxLength(SubscriptionFieldLengths.MaxStatusLength);
    }
}
