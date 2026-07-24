using JustTaskTracker.Domain.Calls.Constants;
using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class CallSessionConfiguration : IEntityTypeConfiguration<CallSession>
{
    public void Configure(EntityTypeBuilder<CallSession> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(CallFieldLengths.MaxTitleLength);
        builder.Property(c => c.Topic).HasMaxLength(CallFieldLengths.MaxTopicLength);
        builder.Property(c => c.AcsRoomId).IsRequired();
    }
}
