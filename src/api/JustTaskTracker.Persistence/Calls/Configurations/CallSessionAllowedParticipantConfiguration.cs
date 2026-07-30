using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class CallSessionAllowedParticipantConfiguration : IEntityTypeConfiguration<CallSessionAllowedParticipant>
{
    public void Configure(EntityTypeBuilder<CallSessionAllowedParticipant> builder)
    {
        builder.HasKey(p => new { p.CallSessionId, p.UserId });

        builder.HasOne<CallSession>()
            .WithMany()
            .HasForeignKey(p => p.CallSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
