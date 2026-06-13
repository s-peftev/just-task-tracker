using JustTaskTracker.Domain.Boards.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Boards.Configurations;

public class BoardTaskCommentConfiguration : IEntityTypeConfiguration<BoardTaskComment>
{
    public void Configure(EntityTypeBuilder<BoardTaskComment> builder)
    {
        builder.Property(c => c.Body).HasMaxLength(2000);

        builder.HasOne(c => c.BoardTask)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.BoardTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Author)
            .WithMany()
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.BoardTaskId, c.CreatedAtUtc })
            .HasFilter("[IsDeleted] = 0");
    }
}
