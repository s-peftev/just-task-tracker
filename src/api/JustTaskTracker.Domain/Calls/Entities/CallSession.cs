using JustTaskTracker.Domain.Calls.Enums;
using JustTaskTracker.Domain.Common.Entities;

namespace JustTaskTracker.Domain.Calls.Entities;

public class CallSession : BaseEntity<Guid>
{
    public Guid BoardId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public CallVisibility Visibility { get; set; }
    public string AcsRoomId { get; set; } = string.Empty;
    public CallStatus Status { get; set; }
    public Guid? CurrentPresenterUserId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
}
