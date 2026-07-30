using JustTaskTracker.Domain.Calls.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Calls.Configurations;

public class CallSessionLinkedTaskConfiguration : IEntityTypeConfiguration<CallSessionLinkedTask>
{
    public void Configure(EntityTypeBuilder<CallSessionLinkedTask> builder)
    {
        builder.HasKey(t => new { t.CallSessionId, t.TaskId });

        builder.HasOne(t => t.CallSession)
            .WithMany(s => s.LinkedTasks)
            .HasForeignKey(t => t.CallSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Task)
            .WithMany()
            .HasForeignKey(t => t.TaskId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
