using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class CallParticipantConfiguration : IEntityTypeConfiguration<CallParticipant>
{
    public void Configure(EntityTypeBuilder<CallParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        // AD-12 idempotency: at most one "still active" (LeftAtUtc IS NULL) row per user per
        // session, so a concurrently-processed duplicate CallParticipantAdded can't create two.
        builder.HasIndex(p => new { p.CallSessionId, p.UserId })
            .IsUnique()
            .HasFilter("[LeftAtUtc] IS NULL");
    }
}
