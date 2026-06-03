namespace JustTaskTracker.Domain.Common;

public abstract record PaginatedRequest
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
