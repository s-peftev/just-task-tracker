using JustTaskTracker.Domain.Boards.Entities;

namespace JustTaskTracker.Domain.Calls.Entities;

// Optional topic references (AD-13). TaskId references BoardTask.Id; a task may be linked to
// several call sessions, but only once per session (composite PK).
public class CallSessionLinkedTask
{
    public Guid CallSessionId { get; set; }
    public Guid TaskId { get; set; }

    public CallSession? CallSession { get; set; }
    public BoardTask? Task { get; set; }
}
