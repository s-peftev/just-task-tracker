using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Domain.Boards.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantMyActiveBoardReadModelConfiguration
    : IEntityTypeConfiguration<AssistantMyActiveBoardReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantMyActiveBoardReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_MyActiveBoards");

        builder.Property(x => x.BoardName).HasMaxLength(BoardFieldLengths.MaxNameLength);
    }
}
