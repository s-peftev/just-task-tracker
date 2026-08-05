using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Domain.Boards.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantAssignedTaskReadModelConfiguration
    : IEntityTypeConfiguration<AssistantAssignedTaskReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantAssignedTaskReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_AssignedTasks");

        builder.Property(x => x.Title).HasMaxLength(BoardTaskFieldLengths.MaxTitleLength);
    }
}
