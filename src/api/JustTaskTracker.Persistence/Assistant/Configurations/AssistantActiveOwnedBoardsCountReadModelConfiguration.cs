using JustTaskTracker.Domain.Assistant.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantActiveOwnedBoardsCountReadModelConfiguration
    : IEntityTypeConfiguration<AssistantActiveOwnedBoardsCountReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantActiveOwnedBoardsCountReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_ActiveOwnedBoardsCount");
    }
}
