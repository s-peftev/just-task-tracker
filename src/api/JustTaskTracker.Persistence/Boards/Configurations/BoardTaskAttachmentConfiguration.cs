using JustTaskTracker.Domain.Boards.Constants;
using JustTaskTracker.Domain.Boards.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JustTaskTracker.Persistence.Boards.Configurations;

public class BoardTaskAttachmentConfiguration : IEntityTypeConfiguration<BoardTaskAttachment>
{
    public void Configure(EntityTypeBuilder<BoardTaskAttachment> builder)
    {
        builder.Property(a => a.OriginalFileName).HasMaxLength(BoardTaskAttachmentFieldLengths.MaxOriginalFileNameLength);
        builder.Property(a => a.ContentType).HasMaxLength(BoardTaskAttachmentFieldLengths.MaxContentTypeLength);
        builder.Property(a => a.BlobName).HasMaxLength(BoardTaskAttachmentFieldLengths.MaxBlobNameLength);

        builder.HasOne(a => a.BoardTask)
            .WithMany(t => t.Attachments)
            .HasForeignKey(a => a.BoardTaskId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.UploadedBy)
            .WithMany()
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => new { a.BoardTaskId, a.Position })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(a => a.BlobName)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
