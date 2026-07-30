using JustTaskTracker.Domain.Boards.Entities;
using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class CallSessionLinkedTaskConfiguration : IEntityTypeConfiguration<CallSessionLinkedTask>
{
    public void Configure(EntityTypeBuilder<CallSessionLinkedTask> builder)
    {
        builder.HasKey(t => new { t.CallSessionId, t.TaskId });

        builder.HasOne<CallSession>()
            .WithMany()
            .HasForeignKey(t => t.CallSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<BoardTask>()
            .WithMany()
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
