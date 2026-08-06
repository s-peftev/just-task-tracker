using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Domain.Boards.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantBoardContextReadModelConfiguration
    : IEntityTypeConfiguration<AssistantBoardContextReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantBoardContextReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_BoardContext");

        builder.Property(x => x.BoardName).HasMaxLength(BoardFieldLengths.MaxNameLength);
    }
}
