using JustTaskTracker.Domain.Assistant.ReadModels;
using JustTaskTracker.Domain.Boards.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Assistant.Configurations;

public class AssistantMyBoardReadModelConfiguration
    : IEntityTypeConfiguration<AssistantMyBoardReadModel>
{
    public void Configure(EntityTypeBuilder<AssistantMyBoardReadModel> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_Assistant_MyBoards");

        builder.Property(x => x.BoardName).HasMaxLength(BoardFieldLengths.MaxNameLength);
    }
}
