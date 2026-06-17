using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Boards.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.ToTable("Columns");

        builder.Property(c => c.Name).HasMaxLength(ColumnFieldLengths.MaxNameLength);

        builder.HasOne(c => c.Board)
            .WithMany(b => b.Columns)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.BoardId, c.Position })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => new { c.BoardId, c.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
