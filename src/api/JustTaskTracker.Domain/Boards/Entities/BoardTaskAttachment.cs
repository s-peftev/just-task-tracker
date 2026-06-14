using JustTaskTracker.Domain.Auth.Entities;
using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Interfaces;

namespace JustTaskTracker.Domain.Boards.Entities;

public class BoardTaskAttachment : AuditableEntity<Guid>, IPositionedEntity
{
    public required Guid BoardTaskId { get; init; }
    public required Guid UploadedById { get; init; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string BlobName { get; set; }
    public int Position { get; set; }

    public BoardTask? BoardTask { get; set; }
    public User? UploadedBy { get; set; }
}
