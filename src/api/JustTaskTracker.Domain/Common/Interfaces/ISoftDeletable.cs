namespace JustTaskTracker.Domain.Common.Interfaces;

public interface ISoftDeletable
{
    DateTime? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}